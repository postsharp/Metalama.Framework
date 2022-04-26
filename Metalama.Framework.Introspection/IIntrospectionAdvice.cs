// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionAdvice
{
    IDeclaration TargetDeclaration { get; }

    string AspectLayerId { get; }

    ImmutableArray<object> Transformations { get; }
}