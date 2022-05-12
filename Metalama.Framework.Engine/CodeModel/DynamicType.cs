﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Types;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class DynamicType : RoslynType<IDynamicTypeSymbol>, IDynamicType
    {
        internal DynamicType( IDynamicTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        public override TypeKind TypeKind => TypeKind.Dynamic;

        public override ITypeInternal Accept( TypeRewriter visitor ) => visitor.Visit( this );
    }
}