// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Types;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel;

internal class FunctionPointerType : RoslynType<IFunctionPointerTypeSymbol>, IFunctionPointerType
{
    public FunctionPointerType( IFunctionPointerTypeSymbol symbol, CompilationModel compilation ) : base( symbol, compilation ) { }

    public override TypeKind TypeKind => TypeKind.FunctionPointer;

    public override ITypeInternal Accept( TypeRewriter visitor ) => visitor.Visit( this );
}