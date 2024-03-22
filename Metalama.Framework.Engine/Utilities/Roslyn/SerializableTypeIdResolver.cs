// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.SyntaxGeneration;
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
    public const string LegacyPrefix = "typeof";
    public const string Prefix = "Y:"; // T: is used for named types.

    private readonly ConcurrentDictionary<SerializableTypeId, ITypeSymbol> _cache = new();
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
    }

    public ITypeSymbol ResolveId( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? genericArguments = null )
    {
        if ( genericArguments == null || genericArguments.Count == 0 )
        {
            return this._cache.GetOrAdd( typeId, static ( id, me ) => me.ResolveCore( id ), this );
        }
        else
        {
            return this.ResolveCore( typeId, genericArguments );
        }
    }

    private ITypeSymbol ResolveCore( SerializableTypeId id, IReadOnlyDictionary<string, IType>? genericArguments = null )
    {
        try
        {
            var idString = id.Id;

            var nullOblivious = idString[^1] != '!';

            if ( !nullOblivious )
            {
                idString = idString[..^1];
            }

            var resolver = new Resolver( this._compilation, genericArguments, isNullOblivious: nullOblivious );

            TypeSyntax type;

            if ( idString.StartsWith( LegacyPrefix, StringComparison.Ordinal ) )
            {
                // Backward compatibility.
                var expression = (TypeOfExpressionSyntax) SyntaxFactoryEx.ParseExpressionSafe( idString );
                type = expression.Type;
            }
            else if ( idString.StartsWith( Prefix, StringComparison.Ordinal ) )
            {
                var method = (MethodDeclarationSyntax?) SyntaxFactory.ParseMemberDeclaration( idString.Substring( Prefix.Length ) + " M();" );

                if ( method == null )
                {
                    // That should not happen even in case of invalid string.
                    throw new AssertionFailedException( $"The string '{idString}' could not be parsed as a type." );
                }
                
                var diagnostics = method.GetDiagnostics().ToArray();

                if ( diagnostics.HasError() )
                {
                    throw new DiagnosticException( $"The string '{idString}' could not be parsed as a type.", diagnostics.ToImmutableArray(), false );
                }

                type = method.ReturnType;
            }
            else
            {
                throw new ArgumentOutOfRangeException( nameof(id) );
            }

            return resolver.Visit( type ) ?? throw new InvalidOperationException( $"Cannot resolve the type '{id}': the resolver returned null." );
        }
        catch ( Exception e )
        {
            throw new InvalidOperationException( $"Cannot resolve the type '{id}': {e.Message}" );
        }
    }

    private sealed class Resolver : SafeSyntaxVisitor<ITypeSymbol?>
    {
        private readonly Compilation _compilation;
        private readonly IReadOnlyDictionary<string, IType>? _genericArguments;
        private readonly bool _isNullOblivious;
        private INamedTypeSymbol? _currentGenericType;

        public Resolver( Compilation compilation, IReadOnlyDictionary<string, IType>? genericArguments, bool isNullOblivious )
        {
            this._compilation = compilation;
            this._genericArguments = genericArguments;
            this._isNullOblivious = isNullOblivious;
        }

        public override ITypeSymbol VisitArrayType( ArrayTypeSyntax node )
            => this._compilation.CreateArrayTypeSymbol( this.Visit( node.ElementType )!, node.RankSpecifiers.Count );

        public override ITypeSymbol VisitPointerType( PointerTypeSyntax node ) => this._compilation.CreatePointerTypeSymbol( this.Visit( node.ElementType )! );

        public override ITypeSymbol VisitNullableType( NullableTypeSyntax node )
        {
            var elementType = this.Visit( node.ElementType ).AssertNotNull();

            return elementType.IsValueType
                ? this._compilation.GetSpecialType( SpecialType.System_Nullable_T ).Construct( elementType )
                : elementType.WithNullableAnnotation( NullableAnnotation.Annotated );
        }

        private INamespaceOrTypeSymbol? LookupName( string name, int arity, INamespaceOrTypeSymbol? ns )
        {
            if ( name == "dynamic" )
            {
                return this._compilation.DynamicType;
            }

            if ( ns == null )
            {
                if ( this._genericArguments != null && this._genericArguments.TryGetValue( name, out var type ) )
                {
                    return type.GetSymbol();
                }
                else if ( this._currentGenericType != null )
                {
                    var typeParameter = this._currentGenericType.TypeParameters.FirstOrDefault( t => t.Name == name );

                    if ( typeParameter != null )
                    {
                        return null;
                    }
                }
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

        private ITypeSymbol? LookupName( NameSyntax name )
        {
            var result = (ITypeSymbol) this.LookupName( name, null );

            if ( !this._isNullOblivious && result != null )
            {
                result = result.WithNullableAnnotation( NullableAnnotation.NotAnnotated );
            }

            return result;
        }

        private INamespaceOrTypeSymbol? LookupName( NameSyntax name, INamespaceOrTypeSymbol? ns )
        {
            switch ( name )
            {
                case IdentifierNameSyntax identifierName:
                    return this.LookupName( identifierName.Identifier.Text, 0, ns );

                case QualifiedNameSyntax qualifiedName:
                    var left = this.LookupName( qualifiedName.Left, ns ).AssertNotNull();

                    return this.LookupName( qualifiedName.Right, left );

                case GenericNameSyntax genericName:
                    var definition = (INamedTypeSymbol) this.LookupName( genericName.Identifier.Text, genericName.Arity, ns );

                    if ( genericName.IsUnboundGenericName )
                    {
                        return definition;
                    }
                    else
                    {
                        var previousGenericType = this._currentGenericType;
                        this._currentGenericType = definition;

                        var typeArguments = genericName.TypeArgumentList.Arguments.SelectAsArray( this.Visit );

                        this._currentGenericType = previousGenericType;

                        if ( Array.IndexOf( typeArguments, null ) >= 0 )
                        {
                            // One of the type argument is the type parameter.
                            return definition;
                        }

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
        {
            ITypeSymbol result = node.Keyword.Kind() switch
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

            if ( !this._isNullOblivious && node.Keyword.Kind() is SyntaxKind.ObjectKeyword or SyntaxKind.StringKeyword )
            {
                result = result.WithNullableAnnotation( NullableAnnotation.NotAnnotated );
            }

            return result;
        }
    }
}