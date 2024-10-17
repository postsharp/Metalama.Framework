﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Source.ConstructedTypes
{
    internal sealed class SymbolPointerType : SymbolConstructedType<IPointerTypeSymbol>, IPointerType
    {
        internal SymbolPointerType( IPointerTypeSymbol typeSymbol, CompilationModel compilation, GenericContext? genericContext ) : base(
            typeSymbol,
            compilation,
            genericContext ) { }

        public override TypeKind TypeKind => TypeKind.Pointer;

        [Memo]
        public IType PointedAtType => this.Compilation.Factory.GetIType( this.Symbol.PointedAtType, this.GenericContextForSymbolMapping );

        public override IType Accept( TypeRewriter visitor ) => visitor.Visit( this );

        internal ITypeImpl WithPointedAtType( IType pointedAtType )
        {
            if ( pointedAtType == this.PointedAtType )
            {
                return this;
            }
            else if ( pointedAtType is ISymbolBasedCompilationElement { Symbol: ITypeSymbol typeSymbol } )
            {
                var symbol = this.Compilation.RoslynCompilation.CreatePointerTypeSymbol( typeSymbol );

                return (ITypeImpl) this.Compilation.Factory.GetIType( symbol );
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}