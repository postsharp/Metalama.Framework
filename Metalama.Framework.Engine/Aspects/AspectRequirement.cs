// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Aspects;

internal readonly struct AspectRequirement
{
    public Ref<IDeclaration> TargetDeclaration { get; }

    public IAspectPredecessor Predecessor { get; }

    public AspectRequirement( Ref<IDeclaration> targetDeclaration, IAspectPredecessor predecessor )
    {
        this.TargetDeclaration = targetDeclaration;
        this.Predecessor = predecessor;
    }
}