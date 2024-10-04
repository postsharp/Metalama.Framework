// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal abstract class BuiltNamedDeclaration : BuiltDeclaration, INamedDeclaration
{
    protected BuiltNamedDeclaration( CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext ) { }

    protected abstract NamedDeclarationBuilderData NamedDeclarationBuilder { get; }

    public string Name => this.NamedDeclarationBuilder.Name;
}