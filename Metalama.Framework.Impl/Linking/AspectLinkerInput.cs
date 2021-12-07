// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.Linking
{
    /// <summary>
    /// Input of the aspect linker.
    /// </summary>
    internal readonly struct AspectLinkerInput
    {
        /// <summary>
        /// Gets the input compilation.
        /// </summary>
        public PartialCompilation InitialCompilation { get; }

        /// <summary>
        /// Gets the input compilation model, modified by all aspects.
        /// </summary>
        public CompilationModel CompilationModel { get; }

        /// <summary>
        /// Gets a list of non-observable transformations.
        /// </summary>
        public IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        /// <summary>
        /// Gets a list of ordered aspect layers.
        /// </summary>
        public IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers { get; }

        public IReadOnlyList<ScopedSuppression> DiagnosticSuppressions { get; }

        public CompileTimeProject CompileTimeProject { get; }

        public AspectLinkerInput(
            PartialCompilation initialCompilation,
            CompilationModel compilationModel,
            IReadOnlyList<INonObservableTransformation> nonObservableTransformations,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers,
            IReadOnlyList<ScopedSuppression> suppressions,
            CompileTimeProject compileTimeProject )
        {
            this.InitialCompilation = initialCompilation;
            this.CompilationModel = compilationModel;
            this.NonObservableTransformations = nonObservableTransformations;
            this.OrderedAspectLayers = orderedAspectLayers;
            this.DiagnosticSuppressions = suppressions;
            this.CompileTimeProject = compileTimeProject;
        }
    }
}