// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Stores the "static" configuration of the pipeline, i.e. the things that don't change
    /// when the user code change. This includes the <see cref="CompileTimeProject"/>, the pipeline stages and
    /// the order of layers.
    /// </summary>
    internal record AspectProjectConfiguration(
        ImmutableArray<PipelineStage> Stages,
        ImmutableArray<IBoundAspectClass> AspectClasses,
        ImmutableArray<OrderedAspectLayer> AspectLayers,
        CompileTimeProject? CompileTimeProject,
        CompileTimeProjectLoader CompileTimeProjectLoader,
        ServiceProvider ServiceProvider )
    {
        public AspectProjectConfiguration WithStages( Func<PipelineStage, PipelineStage> stageMapper )
            => new(
                this.Stages.Select( stageMapper ).ToImmutableArray(),
                this.AspectClasses,
                this.AspectLayers,
                this.CompileTimeProject,
                this.CompileTimeProjectLoader,
                this.ServiceProvider );

        private readonly ImmutableDictionary<string, IBoundAspectClass> _aspectClassesByName = AspectClasses.ToImmutableDictionary( c => c.FullName, c => c );

        public IBoundAspectClass GetAspectClass( string typeName ) => this._aspectClassesByName[typeName];

        public UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetService<UserCodeInvoker>();
    }
}