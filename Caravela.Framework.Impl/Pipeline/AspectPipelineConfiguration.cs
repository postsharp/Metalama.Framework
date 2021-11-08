// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Stores the "static" configuration of the pipeline, i.e. the things that don't change
    /// when the user code change. This includes the <see cref="CompileTimeProject"/>, the pipeline stages and
    /// the order of layers.
    /// </summary>
    internal record AspectPipelineConfiguration(
        ImmutableArray<PipelineStageConfiguration> Stages,
        ImmutableArray<IBoundAspectClass> AspectClasses,
        ImmutableArray<OrderedAspectLayer> AspectLayers,
        CompileTimeProject? CompileTimeProject,
        CompileTimeProjectLoader CompileTimeProjectLoader,
        ServiceProvider ServiceProvider )
    {
        public AspectPipelineConfiguration WithServiceProvider( ServiceProvider serviceProvider )
            => new(
                this.Stages,
                this.AspectClasses,
                this.AspectLayers,
                this.CompileTimeProject,
                this.CompileTimeProjectLoader,
                serviceProvider );

        private readonly ImmutableDictionary<string, IBoundAspectClass> _aspectClassesByName = AspectClasses.ToImmutableDictionary( c => c.FullName, c => c );

        public IBoundAspectClass GetAspectClass( string typeName ) => this._aspectClassesByName[typeName];

        public UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetService<UserCodeInvoker>();
    }
}