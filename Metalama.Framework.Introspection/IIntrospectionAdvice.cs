// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionAdvice
{
    IDeclaration TargetDeclaration { get; }

    string AspectLayerId { get; }

    ImmutableArray<object> Transformations { get; }
}