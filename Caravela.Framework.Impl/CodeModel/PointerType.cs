﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Types;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class PointerType : RoslynType<IPointerTypeSymbol>, IPointerType
    {
        internal PointerType( IPointerTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        public override TypeKind TypeKind => TypeKind.Pointer;

        [Memo]
        public IType PointedAtType => this.Compilation.Factory.GetIType( this.Symbol.PointedAtType );
    }
}