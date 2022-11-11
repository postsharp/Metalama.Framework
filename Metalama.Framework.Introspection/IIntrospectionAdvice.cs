// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionAdvice
{
    AdviceKind AdviceKind { get; }

    IDeclaration TargetDeclaration { get; }

    string AspectLayerId { get; }

    ImmutableArray<IIntrospectionTransformation> Transformations { get; }
}

public interface IIntrospectionTransformation
{
    TransformationKind TransformationKind { get; }

    IDeclaration TargetDeclaration { get; }

    string Description { get; }
}