// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.DesignTime.CodeFixes;
using Metalama.Framework.Impl.Fabrics;
using Metalama.Framework.Impl.Utilities;
using Metalama.Framework.Project;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.Pipeline
{
    /// <summary>
    /// Stores the "static" configuration of the pipeline, i.e. the things that don't change
    /// when the user code change. This includes the <see cref="CompileTimeProject"/>, the pipeline stages and
    /// the order of layers.
    /// </summary>
    internal record AspectPipelineConfiguration(
        ImmutableArray<PipelineStageConfiguration> Stages,
        BoundAspectClassCollection AspectClasses,
        ImmutableArray<OrderedAspectLayer> AspectLayers,
        CompileTimeProject? CompileTimeProject,
        CompileTimeProjectLoader CompileTimeProjectLoader,
        FabricsConfiguration? FabricsConfiguration,
        ProjectModel ProjectModel,
        ServiceProvider ServiceProvider,
        CodeFixFilter CodeFixFilter )
    {
        public AspectPipelineConfiguration WithServiceProvider( ServiceProvider serviceProvider )
            => new(
                this.Stages,
                this.AspectClasses,
                this.AspectLayers,
                this.CompileTimeProject,
                this.CompileTimeProjectLoader,
                this.FabricsConfiguration,
                this.ProjectModel,
                serviceProvider,
                this.CodeFixFilter );

        public AspectPipelineConfiguration WithCodeFixFilter( CodeFixFilter codeFixFilter )
            => codeFixFilter == this.CodeFixFilter
                ? this
                : new AspectPipelineConfiguration(
                    this.Stages,
                    this.AspectClasses,
                    this.AspectLayers,
                    this.CompileTimeProject,
                    this.CompileTimeProjectLoader,
                    this.FabricsConfiguration,
                    this.ProjectModel,
                    this.ServiceProvider,
                    codeFixFilter );

        public UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetService<UserCodeInvoker>();
    }
}