// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Options;

[CompileTime]
public readonly struct AspectOptionsOverrideContext
{
    public AspectOptionsOverrideAxis Axis { get; }

    public IDeclaration Declaration { get; }

    internal AspectOptionsOverrideContext( AspectOptionsOverrideAxis axis, IDeclaration declaration )
    {
        this.Axis = axis;
        this.Declaration = declaration;
    }
}