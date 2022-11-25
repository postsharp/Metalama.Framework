// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal class SerializableTypeIdProvider
{
    private readonly ConcurrentDictionary<SerializableTypeId, ITypeSymbol> _cache = new();
    private readonly Resolver _resolver;

    public SerializableTypeIdProvider( Compilation compilation )
    {
#if DEBUG
        if ( compilation.AssemblyName == "empty" )
        {
            throw new AssertionFailedException( "Expected a non-empty assembly." );
        }
#endif
        this._resolver = new Resolver( compilation );
    }

    public static SerializableTypeId GetId( ITypeSymbol symbol )
    {
        return new SerializableTypeId( OurSyntaxGenerator.CompileTime.TypeOfExpression( symbol ).ToString() );
    }

    public ITypeSymbol ResolveId( SerializableTypeId typeId ) => this._cache.GetOrAdd( typeId, this.ResolveCore );

    private ITypeSymbol ResolveCore( SerializableTypeId id )
    {
        try
        {
            var expression = (TypeOfExpressionSyntax) SyntaxFactory.ParseExpression( id.ToString() );

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

    private class Resolver : SafeSyntaxVisitor<ITypeSymbol>
    {
        private readonly Compilation _compilation;

        public Resolver( Compilation compilation )
        {
            this._compilation = compilation;
        }

        public override ITypeSymbol? VisitArrayType( ArrayTypeSyntax node )
            => this._compilation.CreateArrayTypeSymbol( this.Visit( node.ElementType )!, node.RankSpecifiers.Count );

        public override ITypeSymbol? VisitPointerType( PointerTypeSyntax node ) => this._compilation.CreatePointerTypeSymbol( this.Visit( node.ElementType )! );

        private static INamespaceOrTypeSymbol LookupName( string name, int arity, INamespaceOrTypeSymbol ns )
        {
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

        private ITypeSymbol? LookupName( NameSyntax name ) => (ITypeSymbol) this.LookupName( name, this._compilation.GlobalNamespace );

        private INamespaceOrTypeSymbol LookupName( NameSyntax name, INamespaceOrTypeSymbol ns )
        {
            switch ( name )
            {
                case IdentifierNameSyntax identifierName:
                    return LookupName( identifierName.Identifier.Text, 0, ns );

                case QualifiedNameSyntax qualifiedName:
                    var left = this.LookupName( qualifiedName.Left, ns );

                    return this.LookupName( qualifiedName.Right, left );

                case GenericNameSyntax genericName:
                    var definition = (INamedTypeSymbol) LookupName( genericName.Identifier.Text, genericName.Arity, ns );

                    if ( genericName.IsUnboundGenericName )
                    {
                        return definition;
                    }
                    else
                    {
                        var typeArguments = genericName.TypeArgumentList.Arguments.SelectArray( a => this.Visit( a )! );

                        return definition.Construct( typeArguments );
                    }

                case AliasQualifiedNameSyntax aliasQualifiedName:
                    return this.LookupName( aliasQualifiedName.Name, ns );

                default:
                    throw new InvalidOperationException( $"Unexpected syntax kind: {name.Kind()}." );
            }
        }

        public override ITypeSymbol? VisitGenericName( GenericNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol? VisitAliasQualifiedName( AliasQualifiedNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol? VisitQualifiedName( QualifiedNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol? VisitIdentifierName( IdentifierNameSyntax node ) => this.LookupName( node );

        public override ITypeSymbol? DefaultVisit( SyntaxNode node ) => throw new InvalidOperationException( $"Unexpected node {node.Kind()}." );

        public override ITypeSymbol? VisitPredefinedType( PredefinedTypeSyntax node )
            => node.Keyword.Kind() switch
            {
                SyntaxKind.VoidKeyword => this._compilation.GetSpecialType( SpecialType.System_Void ),
                _ => throw new InvalidOperationException( $"Unexpected predefined type: {node.Keyword.Kind()}" )
            };
    }
}