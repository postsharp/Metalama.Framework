// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public sealed class SerializableTypeIdResolver
{
    private readonly ConcurrentDictionary<SerializableTypeId, ITypeSymbol> _cache = new();
    private readonly Resolver _resolver;
    private readonly Compilation _compilation;

    internal SerializableTypeIdResolver( Compilation compilation )
    {
#if DEBUG
        if ( compilation.AssemblyName == "empty" )
        {
            throw new AssertionFailedException( "Expected a non-empty assembly." );
        }
#endif
        this._compilation = compilation;
        this._resolver = new Resolver( compilation, null );
    }

    public ITypeSymbol ResolveId( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? genericArguments = null )
    {
        if ( genericArguments == null || genericArguments.Count == 0 )
        {
            return this._cache.GetOrAdd( typeId, this.ResolveCore );
        }
        else
        {
            return this.ResolveCore( typeId, genericArguments );
        }
    }

    private ITypeSymbol ResolveCore( SerializableTypeId id )
    {
        try
        {
            var expression = (TypeOfExpressionSyntax) SyntaxFactoryEx.ParseExpressionSafe( id.ToString() );

            var symbol = this._resolver.Visit( expression.Type );

            if ( symbol == null )
            {
                throw new InvalidOperationException( $"Cannot resolve the type '{id}': the resolver returned null." );
            }

            return symbol;
        }
        catch ( Exception e )
        {
            throw new InvalidOperationException( $"Cannot resolve the type '{id}': {e.Message}" );
        }
    }

    private ITypeSymbol ResolveCore( SerializableTypeId id, IReadOnlyDictionary<string, IType> genericArguments )
    {
        try
        {
            var expression = (TypeOfExpressionSyntax) SyntaxFactoryEx.ParseExpressionSafe( id.ToString() );

            var resolver = new Resolver( this._compilation, genericArguments );
            var symbol = resolver.Visit( expression.Type );

            if ( symbol == null )
            {
                throw new InvalidOperationException( $"Cannot resolve the type '{id}': the resolver returned null." );
            }

            return symbol;
        }
        catch ( Exception e )
        {
            throw new InvalidOperationException( $"Cannot resolve the type '{id}': {e.Message}" );
        }
    }

    private sealed class Resolver : SafeSyntaxVisitor<ITypeSymbol>
    {
        private readonly Compilation _compilation;
        private readonly IReadOnlyDictionary<string, IType>? _genericArguments;

        public Resolver( Compilation compilation, IReadOnlyDictionary<string, IType>? genericArguments )
        {
            this._compilation = compilation;
            this._genericArguments = genericArguments;
        }

        public override ITypeSymbol VisitArrayType( ArrayTypeSyntax node )
            => this._compilation.CreateArrayTypeSymbol( this.Visit( node.ElementType )!, node.RankSpecifiers.Count );

        public override ITypeSymbol VisitPointerType( PointerTypeSyntax node ) => this._compilation.CreatePointerTypeSymbol( this.Visit( node.ElementType )! );

        private INamespaceOrTypeSymbol LookupName( string name, int arity, INamespaceOrTypeSymbol? ns )
        {
            if ( ns == null && this._genericArguments != null && this._genericArguments.TryGetValue( name, out var type ) )
            {
                return type.GetSymbol();
            }

            ns ??= this._compilation.GlobalNamespace;

            var candidates = ns.GetMembers( name );

            foreach ( var member in candidates )
            {
                var memberArity = member.Kind == SymbolKind.Namespace ? 0 : ((INamedTypeSymbol) member).Arity;

                if ( arity == memberArity )
                {
                    return (INamespaceOrTypeSymbol) member;
                }
            }

            throw new InvalidOperationException( $"The type or namespace '{ns}' does not contain a member named '{name}' of arity {arity}." );
        }

        private ITypeSymbol LookupName( NameSyntax name ) => (ITypeSymbol) this.LookupName( name, null );

        private INamespaceOrTypeSymbol LookupName( NameSyntax name, INamespaceOrTypeSymbol? ns )
        {
            switch ( name )
            {
                case IdentifierNameSyntax identifierName:
                    return this.LookupName( identifierName.Identifier.Text, 0, ns );

                case QualifiedNameSyntax qualifiedName:
                    var left = this.LookupName( qualifiedName.Left, ns );

                    return this.LookupName( qualifiedName.Right, left );

                case GenericNameSyntax genericName:
                    var definition = (INamedTypeSymbol) this.LookupName( genericName.Identifier.Text, genericName.Arity, ns );

                    if ( genericName.IsUnboundGenericName )
                    {
                        return definition;
                    }
                    else
                    {
                        var typeArguments = genericName.TypeArgumentList.Arguments.SelectAsArray( a => this.Visit( a )! );

                        return definition.Construct( typeArguments );
                    }

                case AliasQualifiedNameSyntax aliasQualifiedName:
                    return this.LookupName( aliasQualifiedName.Name, ns );

                default:
                    throw new InvalidOperationException( $"Unexpected syntax kind: {name.Kind()}." );
            }
        }

        public override ITypeSymbol VisitGenericName( GenericNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol VisitAliasQualifiedName( AliasQualifiedNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol VisitQualifiedName( QualifiedNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol VisitIdentifierName( IdentifierNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol DefaultVisit( SyntaxNode node ) => throw new InvalidOperationException( $"Unexpected node {node.Kind()}." );

        public override ITypeSymbol VisitPredefinedType( PredefinedTypeSyntax node )
            => node.Keyword.Kind() switch
            {
                SyntaxKind.VoidKeyword => this._compilation.GetSpecialType( SpecialType.System_Void ),
                SyntaxKind.BoolKeyword => this._compilation.GetSpecialType( SpecialType.System_Boolean ),
                SyntaxKind.CharKeyword => this._compilation.GetSpecialType( SpecialType.System_Char ),
                SyntaxKind.ObjectKeyword => this._compilation.GetSpecialType( SpecialType.System_Object ),
                SyntaxKind.IntKeyword => this._compilation.GetSpecialType( SpecialType.System_Int32 ),
                SyntaxKind.UIntKeyword => this._compilation.GetSpecialType( SpecialType.System_UInt32 ),
                SyntaxKind.ShortKeyword => this._compilation.GetSpecialType( SpecialType.System_Int16 ),
                SyntaxKind.UShortKeyword => this._compilation.GetSpecialType( SpecialType.System_UInt16 ),
                SyntaxKind.ByteKeyword => this._compilation.GetSpecialType( SpecialType.System_Byte ),
                SyntaxKind.SByteKeyword => this._compilation.GetSpecialType( SpecialType.System_SByte ),
                SyntaxKind.LongKeyword => this._compilation.GetSpecialType( SpecialType.System_Int64 ),
                SyntaxKind.ULongKeyword => this._compilation.GetSpecialType( SpecialType.System_UInt64 ),
                SyntaxKind.FloatKeyword => this._compilation.GetSpecialType( SpecialType.System_Single ),
                SyntaxKind.DoubleKeyword => this._compilation.GetSpecialType( SpecialType.System_Double ),
                SyntaxKind.DecimalKeyword => this._compilation.GetSpecialType( SpecialType.System_Decimal ),
                SyntaxKind.StringKeyword => this._compilation.GetSpecialType( SpecialType.System_String ),

                _ => throw new InvalidOperationException( $"Unexpected predefined type: {node.Keyword.Kind()}" )
            };
    }
}