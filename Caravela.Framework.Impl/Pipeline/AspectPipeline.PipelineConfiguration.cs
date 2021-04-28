// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    public abstract partial class AspectPipeline
    {
        /// <summary>
        /// Stores the "static" configuration of the pipeline, i.e. the things that don't change
        /// when the user code change. This includes the <see cref="CompileTimeProject"/>, the pipeline stages and
        /// the order of layers.
        /// </summary>
        /// <param name="Stages"></param>
        /// <param name="AspectClasses"></param>
        /// <param name="Layers"></param>
        /// <param name="CompileTimeProject"></param>
        /// <param name="CompileTimeProjectLoader"></param>
        private protected record PipelineConfiguration(
            ImmutableArray<PipelineStage> Stages,
            IReadOnlyList<AspectClassMetadata> AspectClasses,
            ImmutableArray<OrderedAspectLayer> Layers,
            CompileTimeProject? CompileTimeProject,
            CompileTimeProjectLoader CompileTimeProjectLoader );
    }
}