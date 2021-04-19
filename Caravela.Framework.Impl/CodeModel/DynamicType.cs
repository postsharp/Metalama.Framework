// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class DynamicType : RoslynType<IDynamicTypeSymbol>, IDynamicType
    {
        internal DynamicType( IDynamicTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        public override TypeKind TypeKind => TypeKind.Dynamic;
    }
}