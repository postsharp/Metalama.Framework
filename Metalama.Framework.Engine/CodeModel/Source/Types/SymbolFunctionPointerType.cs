// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Source.Types;

internal sealed class SymbolFunctionPointerType : SymbolType<IFunctionPointerTypeSymbol>, IFunctionPointerType
{
    public SymbolFunctionPointerType( IFunctionPointerTypeSymbol symbol, CompilationModel compilation ) : base( symbol, compilation ) { }

    public override TypeKind TypeKind => TypeKind.FunctionPointer;

    public override IType Accept( TypeRewriter visitor ) => visitor.Visit( this );
}