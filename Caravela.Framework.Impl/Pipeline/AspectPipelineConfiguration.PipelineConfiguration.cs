// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Stores the "static" configuration of the pipeline, i.e. the things that don't change
    /// when the user code change. This includes the <see cref="CompileTimeProject"/>, the pipeline stages and
    /// the order of layers.
    /// </summary>
    internal record AspectPipelineConfiguration(
        ImmutableArray<PipelineStage> Stages,
        IReadOnlyList<AspectClassMetadata> AspectClasses,
        ImmutableArray<OrderedAspectLayer> Layers,
        CompileTimeProject? CompileTimeProject,
        CompileTimeProjectLoader CompileTimeProjectLoader )
    {
        public AspectPipelineConfiguration WithStages( Func<PipelineStage, PipelineStage> stageMapper )
            => new AspectPipelineConfiguration(
                this.Stages.Select( s => stageMapper( s ) ).ToImmutableArray(),
                this.AspectClasses,
                this.Layers,
                this.CompileTimeProject,
                this.CompileTimeProjectLoader );
    }
}