// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The context in which an <see cref="AspectPipeline"/> is executed. Gives information about the outside.
    /// </summary>
    public interface IAspectPipelineContext
    {
        // TODO: When called from a diagnostic suppressor, we don't have a way to report diagnostics.

        ImmutableArray<object> Plugins { get; }

        IBuildOptions BuildOptions { get; }

        bool HandleExceptions { get; }
    }
}