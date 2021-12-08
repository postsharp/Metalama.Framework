// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Transformations;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.Pipeline
{
    /// <summary>
    /// A read-only view of <see cref="PipelineStepsState"/>.
    /// </summary>
    internal interface IPipelineStepsResult
    {
        CompilationModel Compilation { get; }

        IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        ImmutableArray<AttributeAspectInstance> InheritableAspectInstances { get; }

        ImmutableUserDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the list of aspect sources that are not a part of the current pipeline stage.
        /// </summary>
        IReadOnlyList<IAspectSource> ExternalAspectSources { get; }
    }
}