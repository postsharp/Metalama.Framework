// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class BaseDeclarationCollection
{
    protected BaseDeclarationCollection( CompilationModel compilation )
    {
        this.Compilation = compilation;
    }

    public CompilationModel Compilation { get; protected set; }

    protected RefFactory RefFactory => this.Compilation.CompilationContext.RefFactory;
}