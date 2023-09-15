// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Options;

public interface IAspectOptionsAttribute
{
    ImmutableArray<Type> SupportedOptionTypes { get; }
}

public interface IAspectOptionsAttribute<out T> : IAspectOptionsAttribute
    where T : AspectOptions
{
    T ToOptions();
}