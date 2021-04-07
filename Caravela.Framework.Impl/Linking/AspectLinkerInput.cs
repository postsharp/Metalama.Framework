// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Input of the aspect linker.
    /// </summary>
    internal struct AspectLinkerInput
    {
        /// <summary>
        /// Gets the input compilation.
        /// </summary>
        public CSharpCompilation InitialCompilation { get; }

        /// <summary>
        /// Gets the input compilation model.
        /// </summary>
        public CompilationModel FinalCompilationModel { get; }

        /// <summary>
        /// Gets a list of non-observable transformations.
        /// </summary>
        public IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        /// <summary>
        /// Gets a list of ordered aspect layers.
        /// </summary>
        public IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers { get; }

        public AspectLinkerInput(
            CSharpCompilation initialCompilation,
            CompilationModel finalCompilationModel,
            IReadOnlyList<INonObservableTransformation> nonObservableTransformations,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers )
        {
            this.InitialCompilation = initialCompilation;
            this.FinalCompilationModel = finalCompilationModel;
            this.NonObservableTransformations = nonObservableTransformations;
            this.OrderedAspectLayers = orderedAspectLayers;
        }
    }
}
