// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.CompileTime.Manifest;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// Stores the "static" configuration of the pipeline, i.e. the things that don't change
/// when the user code changes. This includes the <see cref="CompileTimeProject"/>, the pipeline stages and
/// the order of layers.
/// </summary>
public sealed class AspectPipelineConfiguration
{
    internal CompileTimeDomain Domain { get; }

    internal ImmutableArray<PipelineStageConfiguration> Stages { get; }

    internal BoundAspectClassCollection BoundAspectClasses { get; }

    public IReadOnlyCollection<IAspectClass> AspectClasses => this.BoundAspectClasses;

    public ulong AspectClassesHashCode => this.BoundAspectClasses.HashCode;

    internal ImmutableArray<OrderedAspectLayer> AspectLayers { get; }

    internal CompileTimeProject? CompileTimeProject { get; }

    public DiagnosticManifest? ClosureDiagnosticManifest => this.CompileTimeProject?.ClosureDiagnosticManifest;

    private CompileTimeProjectRepository CompileTimeProjectRepository { get; }

    internal PipelineContributorSources? FabricsConfiguration { get; }

    public ImmutableArray<string> FabricTypeNames { get; }

    public ProjectModel ProjectModel { get; }

    public ProjectServiceProvider ServiceProvider { get; }

    internal CodeFixFilter CodeFixFilter { get; }

    internal AspectPipelineConfiguration(
        CompileTimeDomain domain,
        ImmutableArray<PipelineStageConfiguration> stages,
        BoundAspectClassCollection aspectClasses,
        ImmutableArray<OrderedAspectLayer> aspectLayers,
        CompileTimeProject? compileTimeProject,
        CompileTimeProjectRepository compileTimeProjectRepository,
        PipelineContributorSources? fabricsConfiguration,
        ImmutableArray<string> fabricTypeNames,
        ProjectModel projectModel,
        ProjectServiceProvider serviceProvider,
        CodeFixFilter codeFixFilter )
    {
        this.Domain = domain;
        this.Stages = stages;
        this.BoundAspectClasses = aspectClasses;
        this.AspectLayers = aspectLayers;
        this.CompileTimeProject = compileTimeProject;
        this.CompileTimeProjectRepository = compileTimeProjectRepository;
        this.FabricsConfiguration = fabricsConfiguration;
        this.FabricTypeNames = fabricTypeNames;
        this.ProjectModel = projectModel;
        this.ServiceProvider = serviceProvider;
        this.CodeFixFilter = codeFixFilter;
    }

    public AspectPipelineConfiguration WithServiceProvider( in ProjectServiceProvider serviceProvider )
        => new(
            this.Domain,
            this.Stages,
            this.BoundAspectClasses,
            this.AspectLayers,
            this.CompileTimeProject,
            this.CompileTimeProjectRepository,
            this.FabricsConfiguration,
            this.FabricTypeNames,
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
                this.CompileTimeProjectRepository,
                this.FabricsConfiguration,
                this.FabricTypeNames,
                this.ProjectModel,
                this.ServiceProvider,
                codeFixFilter );

    internal UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetRequiredService<UserCodeInvoker>();
}