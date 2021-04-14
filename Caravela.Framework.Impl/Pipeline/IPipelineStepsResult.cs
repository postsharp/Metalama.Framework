// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A read-only view of <see cref="PipelineStepsState"/>.
    /// </summary>
    internal interface IPipelineStepsResult
    {
        CompilationModel Compilation { get; }

        IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        ImmutableDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the list of aspect sources that are not a part of the current pipeline stage.
        /// </summary>
        IReadOnlyList<IAspectSource> ExternalAspectSources { get; }
    }
}