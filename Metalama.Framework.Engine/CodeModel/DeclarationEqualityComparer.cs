// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

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
        => (x == null && y == null) || (x != null && y != null && this._innerComparer.StructuralSymbolComparer.Equals( x.GetSymbol(), y.GetSymbol() ));

    public bool Equals( INamedType? x, INamedType? y )
        => (x == null && y == null) || (x != null && y != null && this._innerComparer.StructuralSymbolComparer.Equals( x.GetSymbol(), y.GetSymbol() ));

    public int GetHashCode( IType obj ) => this._innerComparer.StructuralSymbolComparer.GetHashCode( obj.GetSymbol() );

    public int GetHashCode( INamedType obj ) => this._innerComparer.StructuralSymbolComparer.GetHashCode( obj.GetSymbol() );

    public bool Is( IType left, IType right, ConversionKind kind ) => this.Is( left.GetSymbol(), right.GetSymbol(), kind );

    public bool Is( IType left, Type right, ConversionKind kind ) => this.Is( left.GetSymbol(), this._reflectionMapper.GetTypeSymbol( right ), kind );

    public bool DerivesFrom( INamedType childType, INamedType baseType, DerivedTypesOptions options = DerivedTypesOptions.Default ) => throw new NotImplementedException();

    private bool Is( ITypeSymbol left, ITypeSymbol right, ConversionKind kind )
    {
        left.ThrowIfBelongsToDifferentCompilationThan( right );

        if ( left == right )
        {
            return true;
        }

        var conversion = this._compilation.ClassifyConversion( left, right );

        switch ( kind )
        {
            case ConversionKind.Implicit:
                return conversion.IsImplicit;

            case ConversionKind.ImplicitReference:
                return conversion is { IsImplicit: true, IsBoxing: false } and { IsUserDefined: false, IsDynamic: false };

            default:
                throw new ArgumentOutOfRangeException( nameof(kind) );
        }
    }
}