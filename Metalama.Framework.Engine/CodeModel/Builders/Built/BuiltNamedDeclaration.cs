// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

internal abstract class BuiltNamedDeclaration : BuiltDeclaration, INamedDeclaration
{
    protected BuiltNamedDeclaration( CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext ) { }

    protected abstract NamedDeclarationBuilder NamedDeclarationBuilder { get; }

    public string Name => this.NamedDeclarationBuilder.Name;
}