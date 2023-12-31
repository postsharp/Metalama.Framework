﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Types;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class DynamicType : RoslynType<IDynamicTypeSymbol>, IDynamicType
    {
        internal DynamicType( IDynamicTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        public override TypeKind TypeKind => TypeKind.Dynamic;

        public override ITypeImpl Accept( TypeRewriter visitor ) => visitor.Visit( this );
    }
}