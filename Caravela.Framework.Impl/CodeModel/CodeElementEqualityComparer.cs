// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CodeElementEqualityComparer : ICodeElementComparer
    {
        private readonly Compilation _compilation;
        private readonly ReflectionMapper _reflectionMapper;

        private readonly CodeElementLinkEqualityComparer<CodeElementLink<ICodeElement>> _innerComparer =
            CodeElementLinkEqualityComparer<CodeElementLink<ICodeElement>>.Instance;

        public CodeElementEqualityComparer( ReflectionMapper reflectionMapper, Compilation compilation )
        {
            this._reflectionMapper = reflectionMapper;
            this._compilation = compilation;
        }

        public bool Equals( ICodeElement x, ICodeElement y ) => this._innerComparer.Equals( x.ToLink(), y.ToLink() );

        public int GetHashCode( ICodeElement obj ) => this._innerComparer.GetHashCode( obj.ToLink() );

        public bool Equals( IType x, IType y ) => SymbolEqualityComparer.Default.Equals( x.GetSymbol(), y.GetSymbol() );

        public int GetHashCode( IType obj ) => SymbolEqualityComparer.Default.GetHashCode( obj.GetSymbol() );

        public bool Is( IType left, IType right )
            => this._compilation.HasImplicitConversion( ((ITypeInternal) left).TypeSymbol, ((ITypeInternal) right).TypeSymbol );

        public bool Is( IType left, Type right ) => this._compilation.HasImplicitConversion( left.GetSymbol(), this._reflectionMapper.GetTypeSymbol( right ) );
    }
}