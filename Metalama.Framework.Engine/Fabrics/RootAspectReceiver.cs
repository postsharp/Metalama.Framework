// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Validation;

namespace Metalama.Framework.Engine.Fabrics;

internal sealed class RootAspectReceiver<T> : AspectReceiver<T, int>
    where T : class, IDeclaration
{
    internal RootAspectReceiver(
        IRef<IDeclaration> containingDeclaration,
        IAspectReceiverParent parent,
        CompilationModelVersion compilationModelVersion ) : base(
        parent.ServiceProvider,
        containingDeclaration,
        compilationModelVersion,
        ( action, context ) => action( (T) containingDeclaration.GetTarget( context.Compilation ), 0, context ) )
    {
        this.Parent = parent;
    }

    protected override IAspectReceiverParent Parent { get; }

    protected override bool ShouldCache => false;
}