﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class DeclarationEqualityComparer : IDeclarationComparer
    {
        private readonly Compilation _compilation;
        private readonly ReflectionMapper _reflectionMapper;

        private readonly DeclarationRefEqualityComparer<DeclarationRef<IDeclaration>> _innerComparer =
            DeclarationRefEqualityComparer<DeclarationRef<IDeclaration>>.Instance;

        public DeclarationEqualityComparer( ReflectionMapper reflectionMapper, Compilation compilation )
        {
            this._reflectionMapper = reflectionMapper;
            this._compilation = compilation;
        }

        public bool Equals( IDeclaration x, IDeclaration y ) => this._innerComparer.Equals( x.ToRef(), y.ToRef() );

        public int GetHashCode( IDeclaration obj ) => this._innerComparer.GetHashCode( obj.ToRef() );

        public bool Equals( IType x, IType y ) => SymbolEqualityComparer.Default.Equals( x.GetSymbol(), y.GetSymbol() );

        public bool Equals( INamedType x, INamedType y ) => SymbolEqualityComparer.Default.Equals( x.GetSymbol(), y.GetSymbol() );

        public int GetHashCode( IType obj ) => SymbolEqualityComparer.Default.GetHashCode( obj.GetSymbol() );

        public int GetHashCode( INamedType obj ) => SymbolEqualityComparer.Default.GetHashCode( obj.GetSymbol() );

        public bool Is( IType left, IType right, ConversionKind kind ) => this.Is( left.GetSymbol(), right.GetSymbol(), kind );

        public bool Is( IType left, Type right, ConversionKind kind ) => this.Is( left.GetSymbol(), this._reflectionMapper.GetTypeSymbol( right ), kind );

        private bool Is( ITypeSymbol left, ITypeSymbol right, ConversionKind kind )
        {
            var conversion = this._compilation.ClassifyConversion( left, right );

            switch ( kind )
            {
                case ConversionKind.Implicit:
                    return conversion.IsImplicit;

                case ConversionKind.ImplicitReference:
                    return conversion.IsImplicit && !conversion.IsBoxing && !conversion.IsUserDefined && !conversion.IsDynamic;

                default:
                    throw new ArgumentOutOfRangeException( nameof(kind) );
            }
        }
    }
}