// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// Stores the "static" configuration of the pipeline, i.e. the things that don't change
    /// when the user code change. This includes the <see cref="CompileTimeProject"/>, the pipeline stages and
    /// the order of layers.
    /// </summary>
    public class AspectPipelineConfiguration
    {
        internal CompileTimeDomain Domain { get; }

        internal ImmutableArray<PipelineStageConfiguration> Stages { get; }

        internal BoundAspectClassCollection BoundAspectClasses { get; }

        public IReadOnlyCollection<IAspectClass> AspectClasses => this.BoundAspectClasses;

        internal ImmutableArray<OrderedAspectLayer> AspectLayers { get; }

        public CompileTimeProject? CompileTimeProject { get; }

        internal CompileTimeProjectLoader CompileTimeProjectLoader { get; }

        internal FabricsConfiguration? FabricsConfiguration { get; }

        public ProjectModel ProjectModel { get; }

        public ServiceProvider ServiceProvider { get; }

        internal CodeFixFilter CodeFixFilter { get; }

        internal AspectPipelineConfiguration(
            CompileTimeDomain domain,
            ImmutableArray<PipelineStageConfiguration> stages,
            BoundAspectClassCollection aspectClasses,
            ImmutableArray<OrderedAspectLayer> aspectLayers,
            CompileTimeProject? compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader,
            FabricsConfiguration? fabricsConfiguration,
            ProjectModel projectModel,
            ServiceProvider serviceProvider,
            CodeFixFilter codeFixFilter )
        {
            this.Domain = domain;
            this.Stages = stages;
            this.BoundAspectClasses = aspectClasses;
            this.AspectLayers = aspectLayers;
            this.CompileTimeProject = compileTimeProject;
            this.CompileTimeProjectLoader = compileTimeProjectLoader;
            this.FabricsConfiguration = fabricsConfiguration;
            this.ProjectModel = projectModel;
            this.ServiceProvider = serviceProvider;
            this.CodeFixFilter = codeFixFilter;
        }

        public AspectPipelineConfiguration WithServiceProvider( ServiceProvider serviceProvider )
            => new(
                this.Domain,
                this.Stages,
                this.BoundAspectClasses,
                this.AspectLayers,
                this.CompileTimeProject,
                this.CompileTimeProjectLoader,
                this.FabricsConfiguration,
                this.ProjectModel,
                serviceProvider,
                this.CodeFixFilter );

        internal AspectPipelineConfiguration WithCodeFixFilter( CodeFixFilter codeFixFilter )
            => codeFixFilter == this.CodeFixFilter
                ? this
                : new AspectPipelineConfiguration(
                    this.Domain,
                    this.Stages,
                    this.BoundAspectClasses,
                    this.AspectLayers,
                    this.CompileTimeProject,
                    this.CompileTimeProjectLoader,
                    this.FabricsConfiguration,
                    this.ProjectModel,
                    this.ServiceProvider,
                    codeFixFilter );

        internal UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetRequiredService<UserCodeInvoker>();
    }
}