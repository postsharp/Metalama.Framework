// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Options;

public abstract class AspectOptions
{
    protected internal virtual AspectOptions? GetDefaultOptions( IProject project ) => null;

    protected internal abstract AspectOptions WithChanges( AspectOptions options );
}