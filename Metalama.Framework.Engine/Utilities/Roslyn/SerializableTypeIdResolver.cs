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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public sealed class SerializableTypeIdResolver
{
    internal const string LegacyPrefix = "typeof";
    internal const string Prefix = "Y:"; // T: is used for named types.

    private readonly ConcurrentDictionary<SerializableTypeId, ResolverResult> _cache = new();
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
        var result = this.ResolveAndCache( typeId, genericArguments );

        if ( !result.IsSuccess )
        {
            throw new InvalidOperationException( result.ErrorMessage );
        }
        else
        {
            return result.TypeSymbol.AssertNotNull();
        }
    }

    public bool TryResolveId( SerializableTypeId typeId, [NotNullWhen( true )] out ITypeSymbol? type ) => this.TryResolveId( typeId, null, out type );

    public bool TryResolveId( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? genericArguments, [NotNullWhen( true )] out ITypeSymbol? type )
    {
        var result = this.ResolveAndCache( typeId, genericArguments );

        if ( !result.IsSuccess )
        {
            type = null;

            return false;
        }
        else
        {
            type = result.TypeSymbol;

            return true;
        }
    }

    private ResolverResult ResolveAndCache( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? genericArguments = null )
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

    private ResolverResult ResolveCore( SerializableTypeId id, IReadOnlyDictionary<string, IType>? genericArguments = null )
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

                if ( method == null || method.GetDiagnostics().HasError() )
                {
                    return ResolverResult.Error( $"The string '{idString}' could not be parsed as a type." );
                }

                type = method.ReturnType;
            }
            else
            {
                return ResolverResult.Error( $"The string '{idString}' has an invalid prefix." );
            }

            return resolver.Visit( type );
        }
        catch ( Exception e )
        {
            throw new InvalidOperationException( $"Cannot resolve the type '{id}': {e.Message}" );
        }
    }

    private readonly struct ResolverResult
    {
        public static ResolverResult TypeParameterOfDeclaration { get; } = new( null );

        private readonly object? _value;

        public ITypeSymbol TypeSymbol => (ITypeSymbol?) this._value ?? throw new InvalidOperationException();

        public INamespaceOrTypeSymbol TypeOrNamespaceSymbol => (INamespaceOrTypeSymbol?) this._value ?? throw new InvalidOperationException();

        public string? ErrorMessage => (string?) this._value;

        public bool IsSuccess => this._value is null or INamespaceOrTypeSymbol;

        public bool IsTypeParameterOfDeclaration => this._value == null;

        private ResolverResult( object? value )
        {
            this._value = value;
        }

        public static ResolverResult Success( INamespaceOrTypeSymbol symbol ) => new( symbol );

        public static ResolverResult Error( string errorMessage ) => new( errorMessage );
    }

    private sealed class Resolver : SafeSyntaxVisitor<ResolverResult>
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

        public override ResolverResult VisitArrayType( ArrayTypeSyntax node )
        {
            var elementTypeResult = this.Visit( node.ElementType );

            if ( !elementTypeResult.IsSuccess )
            {
                return elementTypeResult;
            }

            return ResolverResult.Success( this._compilation.CreateArrayTypeSymbol( elementTypeResult.TypeSymbol, node.RankSpecifiers.Count ) );
        }

        public override ResolverResult VisitPointerType( PointerTypeSyntax node )
        {
            var elementTypeResult = this.Visit( node.ElementType );

            if ( !elementTypeResult.IsSuccess )
            {
                return elementTypeResult;
            }

            return ResolverResult.Success( this._compilation.CreatePointerTypeSymbol( elementTypeResult.TypeSymbol ) );
        }

        public override ResolverResult VisitNullableType( NullableTypeSyntax node )
        {
            var elementTypeResult = this.Visit( node.ElementType );

            if ( !elementTypeResult.IsSuccess )
            {
                return elementTypeResult;
            }

            var elementType = elementTypeResult.TypeSymbol.AssertNotNull();

            return ResolverResult.Success(
                elementType.IsValueType
                    ? this._compilation.GetSpecialType( SpecialType.System_Nullable_T ).Construct( elementType )
                    : elementType.WithNullableAnnotation( NullableAnnotation.Annotated ) );
        }

        private ResolverResult LookupName( string name, int arity, INamespaceOrTypeSymbol? ns )
        {
            if ( name == "dynamic" )
            {
                return ResolverResult.Success( this._compilation.DynamicType );
            }

            if ( ns == null )
            {
                if ( this._genericArguments != null && this._genericArguments.TryGetValue( name, out var type ) )
                {
                    return ResolverResult.Success( type.GetSymbol() );
                }
                else if ( this._currentGenericType != null )
                {
                    var typeParameter = this._currentGenericType.TypeParameters.FirstOrDefault( t => t.Name == name );

                    if ( typeParameter != null )
                    {
                        // We matched the name of a type parameter of the type declaration.
                        return ResolverResult.TypeParameterOfDeclaration;
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
                    return ResolverResult.Success( (INamespaceOrTypeSymbol) member );
                }
            }

            return ResolverResult.Error( $"The type or namespace '{ns}' does not contain a member named '{name}' of arity {arity}." );
        }

        private ResolverResult LookupName( NameSyntax name )
        {
            var result = this.LookupName( name, null );

            if ( !result.IsSuccess )
            {
                return result;
            }
            else if ( result.IsTypeParameterOfDeclaration )
            {
                return result;
            }
            else if ( !this._isNullOblivious )
            {
                return ResolverResult.Success( result.TypeSymbol.WithNullableAnnotation( NullableAnnotation.NotAnnotated ) );
            }
            else
            {
                return result;
            }
        }

        private ResolverResult LookupName( NameSyntax name, INamespaceOrTypeSymbol? ns )
        {
            switch ( name )
            {
                case IdentifierNameSyntax identifierName:
                    return this.LookupName( identifierName.Identifier.Text, 0, ns );

                case QualifiedNameSyntax qualifiedName:
                    var leftResult = this.LookupName( qualifiedName.Left, ns );

                    if ( !leftResult.IsSuccess )
                    {
                        return leftResult;
                    }

                    var left = leftResult.TypeOrNamespaceSymbol.AssertNotNull();

                    return this.LookupName( qualifiedName.Right, left );

                case GenericNameSyntax genericName:
                    var definitionResult = this.LookupName( genericName.Identifier.Text, genericName.Arity, ns );

                    if ( !definitionResult.IsSuccess )
                    {
                        return definitionResult;
                    }

                    var definition = (INamedTypeSymbol) definitionResult.TypeSymbol;

                    if ( genericName.IsUnboundGenericName )
                    {
                        return definitionResult;
                    }
                    else
                    {
                        var previousGenericType = this._currentGenericType;
                        this._currentGenericType = definition;

                        var typeArgumentResults = genericName.TypeArgumentList.Arguments.SelectAsArray( this.Visit );

                        this._currentGenericType = previousGenericType;

                        foreach ( var typeArgumentResult in typeArgumentResults )
                        {
                            if ( !typeArgumentResult.IsSuccess )
                            {
                                return typeArgumentResult;
                            }
                            else if ( typeArgumentResult.IsTypeParameterOfDeclaration )
                            {
                                return definitionResult;
                            }
                        }

                        return ResolverResult.Success( definition.Construct( typeArgumentResults.SelectAsArray( r => r.TypeSymbol.AssertNotNull() ) ) );
                    }

                case AliasQualifiedNameSyntax aliasQualifiedName:
                    return this.LookupName( aliasQualifiedName.Name, ns );

                default:
                    throw new InvalidOperationException( $"Unexpected syntax kind: {name.Kind()}." );
            }
        }

        public override ResolverResult VisitGenericName( GenericNameSyntax node ) => this.LookupName( node );

        public override ResolverResult VisitAliasQualifiedName( AliasQualifiedNameSyntax node ) => this.LookupName( node );

        public override ResolverResult VisitQualifiedName( QualifiedNameSyntax node ) => this.LookupName( node );

        public override ResolverResult VisitIdentifierName( IdentifierNameSyntax node ) => this.LookupName( node );

        public override ResolverResult DefaultVisit( SyntaxNode node ) => throw new InvalidOperationException( $"Unexpected node {node.Kind()}." );

        public override ResolverResult VisitPredefinedType( PredefinedTypeSyntax node )
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

            return ResolverResult.Success( result );
        }
    }
}