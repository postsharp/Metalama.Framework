// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class DeclarationEqualityComparer : IDeclarationComparer
{
    private readonly Compilation _compilation;
    private readonly ReflectionMapper _reflectionMapper;

    private readonly RefEqualityComparer<IDeclaration> _innerComparer;

    public DeclarationEqualityComparer( ReflectionMapper reflectionMapper, Compilation compilation, bool includeNullability )
    {
        this._innerComparer = includeNullability ? RefEqualityComparer<IDeclaration>.IncludeNullability : RefEqualityComparer<IDeclaration>.Default;
        this._reflectionMapper = reflectionMapper;
        this._compilation = compilation;
    }

    public bool Equals( IDeclaration? x, IDeclaration? y )
        => (x == null && y == null) || (x != null && y != null && this._innerComparer.Equals( x.ToTypedRef(), y.ToTypedRef() ));

    public int GetHashCode( IDeclaration obj ) => this._innerComparer.GetHashCode( obj.ToTypedRef() );

    public bool Equals( IType? x, IType? y )
        => (x == null && y == null) || (x != null && y != null && this._innerComparer.StructuralDeclarationComparer.Equals( x, y ));

    public bool Equals( INamedType? x, INamedType? y )
        => (x == null && y == null) || (x != null && y != null && this._innerComparer.StructuralDeclarationComparer.Equals( x, y ));

    public int GetHashCode( IType obj ) => this._innerComparer.StructuralDeclarationComparer.GetHashCode( obj );

    public int GetHashCode( INamedType obj ) => this._innerComparer.StructuralDeclarationComparer.GetHashCode( obj );

    public bool Is( IType left, IType right, ConversionKind kind )
    {
        if ( left.GetSymbol() is { } leftSymbol && right.GetSymbol() is { } rightSymbol )
        {
            return this.Is( leftSymbol, rightSymbol, kind );
        }

        if ( kind is not ConversionKind.Reference and not ConversionKind.Default || left is not INamedType leftType || right is not INamedType
             || right is INamedType { IsGeneric: true } || right is INamedType { TypeKind: Code.TypeKind.Interface } )
        {
            // TODO: Implement.
            throw new NotSupportedException( "Not yet supported on introduced types." );
        }

        var current = leftType;

        while ( current != null )
        {
            if ( this._innerComparer.StructuralDeclarationComparer.Equals( current, right ) )
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    public bool Is( MemberRef<INamedType> left, IType right, ConversionKind kind )
    {
        // This should not instantiate the type if not needed.
        switch ( left.Target )
        {
            case ITypeSymbol symbol:
                return this.Is( symbol, right.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedTypeComparison ), kind );

            case NamedTypeBuilder builder:
                return this.Is( builder, right, kind );

            default:
                throw new AssertionFailedException( $"Unsupported: {left.Target}" );
        }
    }

    public bool Is( IType left, Type right, ConversionKind kind )
        => this.Is(
            left.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedTypeComparison ),
            this._reflectionMapper.GetTypeSymbol( right ),
            kind );

    internal bool Is( ITypeSymbol left, ITypeSymbol right, ConversionKind kind )
    {
        // TODO: Does not take introduced interfaces into account (requires a lot of changes).

        left.ThrowIfBelongsToDifferentCompilationThan( right );

        if ( kind == ConversionKind.TypeDefinition )
        {
            // Cannot use Roslyn for this kind of conversion.

            if ( right is not INamedTypeSymbol rightNamedType || !SymbolEqualityComparer.Default.Equals( rightNamedType, rightNamedType.ConstructedFrom ) )
            {
                throw new ArgumentException(
                    $"{nameof(ConversionKind)}.{nameof(ConversionKind.TypeDefinition)} can only be used with unbound generic type on the right side." );
            }

            switch ( left )
            {
                case INamedTypeSymbol leftNamedType:
                    return IsOfTypeDefinition( leftNamedType, rightNamedType );

                default:
                    return false;
            }
        }

        if ( left == right )
        {
            return true;
        }

        var conversion = this._compilation.ClassifyConversion( left, right );

        switch ( kind )
        {
            case ConversionKind.Implicit:
                return conversion is { IsIdentity: true } or { IsImplicit: true };

            case ConversionKind.Reference:
                return conversion is { IsIdentity: true } or { IsImplicit: true, IsReference: true };

            case ConversionKind.Default:
                return conversion is { IsIdentity: true } or { IsImplicit: true, IsReference: true } or { IsImplicit: true, IsBoxing: true };

            default:
                throw new ArgumentOutOfRangeException( nameof(kind) );
        }
    }

    private static bool IsOfTypeDefinition( INamedTypeSymbol type, INamedTypeSymbol typeDefinition )
    {
        // TODO: This can be optimized (e.g. when searching for interface definition, classes don't have to be checked for symbol equality).

        // Evaluate the current type.

        if ( SymbolEqualityComparer.Default.Equals( type.ConstructedFrom, typeDefinition ) )
        {
            return true;
        }

        if ( typeDefinition is { TypeKind: RoslynTypeKind.Interface } )
        {
            // When searching for an interface, we should consider interfaces defined by the evaluated type.
            if ( type.Interfaces.Any( i => IsOfTypeDefinition( i, typeDefinition ) ) )
            {
                // The type implements interface that has the same definition.
                return true;
            }
        }

        return type.BaseType != null && IsOfTypeDefinition( type.BaseType, typeDefinition );
    }
}