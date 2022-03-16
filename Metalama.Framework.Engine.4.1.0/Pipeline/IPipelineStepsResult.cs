// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// A read-only view of <see cref="PipelineStepsState"/>.
    /// </summary>
    internal interface IPipelineStepsResult
    {
        CompilationModel LastCompilation { get; }

        IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        ImmutableArray<IAspectInstance> InheritableAspectInstances { get; }

        ImmutableUserDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the list of aspect sources that are not a part of the current pipeline stage.
        /// </summary>
        ImmutableArray<IAspectSource> ExternalAspectSources { get; }

        ImmutableArray<IValidatorSource> ValidatorSources { get; }

        ImmutableArray<CompilationModel> Compilations { get; }

        ImmutableArray<AspectInstanceResult> AspectInstanceResults { get; }
    }
}