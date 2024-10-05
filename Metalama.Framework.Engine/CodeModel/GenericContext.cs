// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class GenericContext : IEquatable<GenericContext?>, IGenericContext
{
    public static GenericContext Empty { get; } = new();

    public INamedTypeSymbol? NamedTypeSymbol { get; }

    public IMethodSymbol? MethodSymbol { get; }

    public CompilationContext? CompilationContext { get; }

    public bool IsEmptyOrIdentity => this.NamedTypeSymbol is null;

    // Creates the Empty instance.
    private GenericContext() { }

    private GenericContext( INamedTypeSymbol namedTypeSymbol, IMethodSymbol? methodSymbol, CompilationContext? compilationContext )
    {
        // Assert that we only create a non-empty GenericContext only when we have a non-canonical mapping.
        Invariant.Assert( !namedTypeSymbol.IsDefinitionSafe() || methodSymbol != null );
        Invariant.Assert( methodSymbol == null || !methodSymbol.IsDefinitionSafe() );

        this.NamedTypeSymbol = namedTypeSymbol;
        this.MethodSymbol = methodSymbol;
        this.CompilationContext = compilationContext;
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

    public static GenericContext Get( INamedTypeSymbol? symbol, CompilationContext compilationContext )
    {
        if ( symbol == null || symbol.IsDefinitionSafe() )
        {
            return Empty;
        }

        return new GenericContext( symbol, null, compilationContext );
    }

    public static GenericContext Get( IMethodSymbol? symbol, CompilationContext compilationContext )
    {
        if ( symbol == null || symbol.IsDefinitionSafe() )
        {
            return Empty;
        }

        var genericMethodSymbol = symbol.TypeArguments.IsEmpty ? null : symbol;

        return new GenericContext( symbol.ContainingType, genericMethodSymbol, compilationContext );
    }

    [Memo]
    private TypeSymbolMapper TypeSymbolMapperInstance => new( this );

    [Memo]
    private TypeMapper TypeMapperInstance => new( this );

    [Memo]
    private SymbolMapper SymbolMapperInstance => new( this );

    public ITypeSymbol Map( ITypeParameterSymbol typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        if ( typeParameter.TypeParameterKind == TypeParameterKind.Type )
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
        else if ( typeParameter.TypeParameterKind == TypeParameterKind.Method )
        {
            if ( this.MethodSymbol == null )
            {
                // Cannot map it.
                return typeParameter;
            }
            else
            {
                return this.MethodSymbol.TypeArguments[typeParameter.Ordinal];
            }
        }
        else
        {
            throw new AssertionFailedException();
        }
    }

    public IType Map( ITypeParameter typeParameter )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return typeParameter;
        }

        var mappedSymbol = this.Map( ((TypeParameter) typeParameter).TypeParameterSymbol );

        return typeParameter.GetCompilationModel().Factory.GetIType( mappedSymbol );
    }

    [return: NotNullIfNotNull( nameof(type) )]
    public IType? Map( IType? type )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return type;
        }

        return type switch
        {
            null => null,
            ITypeParameter typeParameter => this.Map( typeParameter ),
            _ when type.SpecialType != SpecialType.None || !TypeVisitor.Instance.Visit( type ) => type, // Fast oath
            _ => this.TypeMapperInstance.Visit( type )
        };
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

    public bool Equals( GenericContext? other )
    {
        if ( other == null )
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals( this.NamedTypeSymbol, other.NamedTypeSymbol );
    }

    public override bool Equals( object? obj ) => obj is GenericContext genericMap && this.Equals( genericMap );

    public override int GetHashCode()
    {
        return SymbolEqualityComparer.Default.GetHashCode( this.NamedTypeSymbol );
    }

    public override string ToString()
    {
        return (this.NamedTypeSymbol, this.MethodSymbol) switch
        {
            (null, null) => "GenericContext (Empty)",
            (_, not null) => $"GenericContext Method={{{this.MethodSymbol.ToDisplayString()}}}",
            _ => $"GenericContext Type={{{this.NamedTypeSymbol.ToDisplayString()}}}"
        };
    }
}