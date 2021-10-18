// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A read-only view of <see cref="PipelineStepsState"/>.
    /// </summary>
    internal interface IPipelineStepsResult
    {
        CompilationModel Compilation { get; }

        IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        IReadOnlyList<AttributeAspectInstance> InheritedAspectInstances { get; }

        ImmutableUserDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the list of aspect sources that are not a part of the current pipeline stage.
        /// </summary>
        IReadOnlyList<IAspectSource> ExternalAspectSources { get; }
    }
}