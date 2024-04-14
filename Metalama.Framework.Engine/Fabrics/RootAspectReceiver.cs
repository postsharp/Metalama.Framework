// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Validation;

namespace Metalama.Framework.Engine.Fabrics;

internal class RootAspectReceiver<T> : AspectReceiver<T, object?>
    where T : class, IDeclaration
{
    internal RootAspectReceiver(
        ISdkRef<IDeclaration> containingDeclaration,
        IAspectReceiverParent parent,
        CompilationModelVersion compilationModelVersion ) : base(
        containingDeclaration,
        parent,
        compilationModelVersion,
        ( action, context ) => action( (T) containingDeclaration.GetTarget( context.Compilation ), null, context ) ) { }

    protected override bool ShouldCache => false;
}