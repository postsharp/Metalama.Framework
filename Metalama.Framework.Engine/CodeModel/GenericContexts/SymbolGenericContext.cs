// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using TypeParameterKind = Microsoft.CodeAnalysis.TypeParameterKind;

namespace Metalama.Framework.Engine.CodeModel.GenericContexts;

/// <summary>
/// Implements a <see cref="GenericContext"/> where all type parameters are mapped to symbols.
/// </summary>
internal partial class SymbolGenericContext : GenericContext
{
    private readonly CompilationContext? _compilationContext;

    public INamedTypeSymbol NamedTypeSymbol { get; }

    public IMethodSymbol? MethodSymbol { get; }

    private SymbolGenericContext( INamedTypeSymbol namedTypeSymbol, IMethodSymbol? methodSymbol, CompilationContext? compilationContext )
    {
        // Assert that we only create a non-empty GenericContext only when we have a non-canonical mapping.
        Invariant.Assert( !namedTypeSymbol.IsDefinitionSafe() || methodSymbol != null );
        Invariant.Assert( methodSymbol == null || !methodSymbol.IsDefinitionSafe() );

        this.NamedTypeSymbol = namedTypeSymbol;
        this.MethodSymbol = methodSymbol;
        this._compilationContext = compilationContext;
    }

    public static GenericContext Get( INamedTypeSymbol? symbol, CompilationContext compilationContext )
    {
        if ( symbol == null || symbol.IsDefinitionSafe() )
        {
            return Empty;
        }

        return new SymbolGenericContext( symbol, null, compilationContext );
    }

    public static GenericContext Get( IMethodSymbol? symbol, CompilationContext compilationContext )
    {
        if ( symbol == null || symbol.IsDefinitionSafe() )
        {
            return Empty;
        }

        var genericMethodSymbol = symbol.TypeArguments.IsEmpty ? null : symbol;

        return new SymbolGenericContext( symbol.ContainingType, genericMethodSymbol, compilationContext );
    }

    public static GenericContext Get( ISymbol? symbol, CompilationContext compilationContext )
    {
        var closestMember = symbol?.GetClosestContainingMember();

        if ( closestMember == null )
        {
            return Empty;
        }

        return closestMember.Kind switch
        {
            SymbolKind.Method => Get( (IMethodSymbol) closestMember, compilationContext ),
            SymbolKind.NamedType => Get( (INamedTypeSymbol) closestMember, compilationContext ),
            _ => Get( closestMember.ContainingType, compilationContext )
        };
    }

    [Memo]
    private TypeSymbolMapper TypeSymbolMapperInstance => new( this );

    [Memo]
    private SymbolMapper SymbolMapperInstance => new( this );

    private ITypeSymbol Map( ITypeParameterSymbol typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        switch ( typeParameter.TypeParameterKind )
        {
            case TypeParameterKind.Type:
                {
                    // Find which type of the stack of nested types we have to consider.
                    var requestedTypeDefinition = typeParameter.DeclaringType!.OriginalDefinition;

                    for ( var type = this.NamedTypeSymbol; type != null; type = type.ContainingType )
                    {
                        if ( type.OriginalDefinition == requestedTypeDefinition )
                        {
                            return type.TypeArguments[typeParameter.Ordinal];
                        }
                    }

                    // The type parameter cannot be matched. This can happen when we are trying to match a nested type A<T1>.B<T2> in the context of A<string>,
                    // i.e. the top-level type is bound and the nested type is unbound.
                    return typeParameter;
                }

            case TypeParameterKind.Method when this.MethodSymbol == null:
                // Cannot map it.
                return typeParameter;

            case TypeParameterKind.Method:
                return this.MethodSymbol.TypeArguments[typeParameter.Ordinal];

            default:
                throw new AssertionFailedException();
        }
    }

    [return: NotNullIfNotNull( nameof(type) )]
    public ITypeSymbol? Map( ITypeSymbol? type )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return type;
        }

        return type switch
        {
            null => null,
            ITypeParameterSymbol typeParameter => this.Map( typeParameter ),
            _ => TypeSymbolVisitor.Instance.Visit( type ) ? this.TypeSymbolMapperInstance.Visit( type ) : type
        };
    }

    [return: NotNullIfNotNull( nameof(symbol) )]
    public ISymbol? Map( ISymbol? symbol )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return symbol;
        }

        if ( symbol == null )
        {
            return null;
        }

        return this.SymbolMapperInstance.Visit( symbol ).AssertSymbolNotNull();
    }

    public override GenericContextKind Kind => GenericContextKind.Symbol;

    public override IType Map( ITypeParameter typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        switch ( typeParameter.TypeParameterKind )
        {
            case Code.TypeParameterKind.Type:
                {
                    // Find which type of the stack of nested types we have to consider.
                    var requestedTypeDefinition = ((INamedType) typeParameter.ContainingDeclaration.AssertNotNull()).Definition.GetSymbol()
                        .AssertSymbolNullNotImplemented( "Generic context of constructed type." );

                    for ( var type = this.NamedTypeSymbol; type != null; type = type.ContainingType )
                    {
                        if ( type.OriginalDefinition == requestedTypeDefinition )
                        {
                            return typeParameter.GetCompilationModel().Factory.GetIType( type.TypeArguments[typeParameter.Index] );
                        }
                    }

                    // The type parameter cannot be matched. This can happen when we are trying to match a nested type A<T1>.B<T2> in the context of A<string>,
                    // i.e. the top-level type is bound and the nested type is unbound.
                    return typeParameter;
                }

            case Code.TypeParameterKind.Method when this.MethodSymbol == null:
                // Cannot map it.
                return typeParameter;

            case Code.TypeParameterKind.Method:
                return typeParameter.GetCompilationModel().Factory.GetIType( this.MethodSymbol.TypeArguments[typeParameter.Index] );

            default:
                throw new AssertionFailedException();
        }
    }

    public override bool Equals( GenericContext? other )
    {
        if ( other is not SymbolGenericContext otherSymbolGenericContect )
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals( this.NamedTypeSymbol, otherSymbolGenericContect.NamedTypeSymbol );
    }

    protected override int GetHashCodeCore() => SymbolEqualityComparer.Default.GetHashCode( this.NamedTypeSymbol );

    public override string ToString()
        => (this.NamedTypeSymbol, this.MethodSymbol) switch
        {
            (_, not null) => $"SymbolGenericContext Method={{{this.MethodSymbol.ToDebugString()}}}",
            _ => $"SymbolGenericContext Type={{{this.NamedTypeSymbol.ToDebugString()}}}"
        };
}