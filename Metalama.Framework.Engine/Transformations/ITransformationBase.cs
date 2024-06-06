// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Introspection;
using System;

namespace Metalama.Framework.Engine.Transformations;

public interface ITransformationBase
{
    IAspectClass AspectClass { get; }

    IDeclaration TargetDeclaration { get; }

    TransformationKind TransformationKind { get; }

    FormattableString ToDisplayString();
}