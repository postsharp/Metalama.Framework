// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

[RunTimeOrCompileTime]
public interface IAspectOptionsAttribute { }

public interface IAspectOptionsAttribute<out T> : IAspectOptionsAttribute
    where T : AspectOptions
{
    T ToOptions();
}