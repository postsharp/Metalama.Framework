// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Validation;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Fabrics;

internal sealed class ChildAspectReceiver<TDeclaration, TTag> : AspectReceiver<TDeclaration, TTag>
    where TDeclaration : class, IDeclaration
{
    internal ChildAspectReceiver(
        IRef<IDeclaration> containingDeclaration,
        IAspectReceiverParent parent,
        CompilationModelVersion compilationModelVersion,
        Func<Func<TDeclaration, TTag, DeclarationSelectionContext, Task>, DeclarationSelectionContext, Task> addTargets )
        : base( parent.ServiceProvider, containingDeclaration, compilationModelVersion, addTargets )
    {
        this.Parent = parent;
    }

    protected override IAspectReceiverParent Parent { get; }
}