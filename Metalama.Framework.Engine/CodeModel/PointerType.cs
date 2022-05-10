// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class PointerType : RoslynType<IPointerTypeSymbol>, IPointerType
    {
        internal PointerType( IPointerTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        public override TypeKind TypeKind => TypeKind.Pointer;

        [Memo]
        public IType PointedAtType => this.Compilation.Factory.GetIType( this.Symbol.PointedAtType );

        internal ITypeInternal WithPointedAtType( ITypeInternal typeInternal )
        {
            throw new NotImplementedException();
        }

        public override ITypeInternal Accept( TypeRewriter visitor ) => visitor.Visit( this );

        internal ITypeInternal WithPointedAt( ITypeInternal pointedAtType )
        {
            if ( pointedAtType == this.PointedAtType )
            {
                return this;
            }
            else
            {
                var symbol = this.GetCompilationModel().RoslynCompilation.CreatePointerTypeSymbol( pointedAtType.GetSymbol() );

                return (ITypeInternal) this.GetCompilationModel().Factory.GetIType( symbol );
            }
        }
    }
}