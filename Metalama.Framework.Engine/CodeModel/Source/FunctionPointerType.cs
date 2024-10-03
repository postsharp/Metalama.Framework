// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Source;

internal sealed class FunctionPointerType : RoslynType<IFunctionPointerTypeSymbol>, IFunctionPointerType
{
    public FunctionPointerType( IFunctionPointerTypeSymbol symbol, CompilationModel compilation ) : base( symbol, compilation ) { }

    public override TypeKind TypeKind => TypeKind.FunctionPointer;

    public override IType Accept( TypeRewriter visitor ) => visitor.Visit( this );
}