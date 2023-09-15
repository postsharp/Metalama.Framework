// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Options;

public abstract class AspectOptions : ICompileTimeSerializable
{
    public virtual AspectOptions? GetDefaultOptions( IProject project ) => null;

    public abstract AspectOptions OverrideWith( AspectOptions options, in AspectOptionsOverrideContext context );
}