﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Source.Types
{
    internal sealed class DynamicType : SymbolType<IDynamicTypeSymbol>, IDynamicType
    {
        internal DynamicType( IDynamicTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation, null ) { }

        public override TypeKind TypeKind => TypeKind.Dynamic;

        public override IType Accept( TypeRewriter visitor ) => visitor.Visit( this );
    }
}