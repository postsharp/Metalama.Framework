// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Fabrics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The base class for the main process of Caravela.
    /// </summary>
    public abstract class AspectPipeline : IDisposable
    {
        private const string _highLevelStageGroupingKey = nameof(_highLevelStageGroupingKey);
        private readonly bool _ownsDomain;

        public IProjectOptions ProjectOptions { get; }

        private readonly CompileTimeDomain _domain;

        // This member is intentionally protected because there can be one ServiceProvider per project,
        // but the pipeline can be used by many projects.
        protected internal ServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectPipeline"/> class.
        /// </summary>
        /// <param name="projectOptions"></param>
        /// <param name="executionScenario"></param>
        /// <param name="isTest"></param>
        /// <param name="domain">If <c>null</c>, the instance is created from the <see cref="ICompileTimeDomainFactory"/> service.</param>
        /// <param name="directoryOptions"></param>
        /// <param name="assemblyLocator"></param>
        protected AspectPipeline(
            ServiceProvider serviceProvider,
            AspectExecutionScenario executionScenario,
            bool isTest,
            CompileTimeDomain? domain )
        {
            this.ProjectOptions = serviceProvider.GetService<IProjectOptions>();

            this.ServiceProvider = serviceProvider
                .WithServices( this.ProjectOptions.PlugIns.OfType<IService>() )
                .WithServices( new AspectPipelineDescription( executionScenario, isTest ) )
                .WithMark( ServiceProviderMark.Pipeline );

            if ( domain != null )
            {
                this._domain = domain;
            }
            else
            {
                // Coverage: Ignore (tests always provide a domain).
                this._domain = this.ServiceProvider.GetService<ICompileTimeDomainFactory>().CreateDomain();
                this._ownsDomain = true;
            }
        }

        internal int PipelineInitializationCount { get; private set; }

        private protected bool TryInitialize(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            this.PipelineInitializationCount++;

            var roslynCompilation = compilation.Compilation;

            // Create dependencies.
            var loader = CompileTimeProjectLoader.Create( this._domain, this.ServiceProvider );

            // Prepare the compile-time assembly.
            if ( !loader.TryGetCompileTimeProjectFromCompilation(
                roslynCompilation,
                compileTimeTreesHint,
                diagnosticAdder,
                false,
                cancellationToken,
                out var compileTimeProject ) )
            {
                configuration = null;

                return false;
            }

            // Create a project-level service provider.
            var projectServiceProviderWithoutPlugins = this.ServiceProvider.WithService( loader ).WithMark( ServiceProviderMark.Project );
            var projectServiceProviderWithProject = projectServiceProviderWithoutPlugins;

            // Create compiler plug-ins found in compile-time code.
            ImmutableArray<object> compilerPlugIns;

            if ( compileTimeProject != null )
            {
                projectServiceProviderWithProject = projectServiceProviderWithProject.WithService( compileTimeProject );

                // The instantiation of compiler plug-ins defined in the current compilation is a bit rough here, but it is supposed to be used
                // by our internal tests only. However, the logic will interfere with production scenario, where a plug-in will be both
                // in ProjectOptions.PlugIns and in CompileTimeProjects.PlugInTypes. So, we do not load the plug ins found by CompileTimeProjects.PlugInTypes
                // if they are already provided by ProjectOptions.PlugIns.

                var invoker = this.ServiceProvider.GetService<UserCodeInvoker>();

                var loadedPlugInsTypes = this.ProjectOptions.PlugIns.Select( t => t.GetType().FullName ).ToImmutableArray();

                var additionalPlugIns = compileTimeProject.ClosureProjects
                    .SelectMany( p => p.PlugInTypes.Select( t => (Project: p, TypeName: t) ) )
                    .Where( t => !loadedPlugInsTypes.Contains( t.TypeName ) )
                    .Select(
                        t =>
                        {
                            var type = t.Project.GetType( t.TypeName );
                            var constructor = type.GetConstructor( Type.EmptyTypes );

                            if ( constructor == null )
                            {
                                diagnosticAdder.Report( GeneralDiagnosticDescriptors.TypeMustHavePublicDefaultConstructor.CreateDiagnostic( null, type ) );

                                return null;
                            }

                            var executionContext = new UserCodeExecutionContext(
                                projectServiceProviderWithoutPlugins,
                                diagnosticAdder,
                                UserCodeMemberInfo.FromMemberInfo( constructor ) );

                            if ( !invoker.TryInvoke( () => Activator.CreateInstance( type ), executionContext, out var instance ) )
                            {
                                return null;
                            }
                            else
                            {
                                return instance;
                            }
                        } )
                    .WhereNotNull()
                    .ToList();

                if ( additionalPlugIns.Count > 0 )
                {
                    // If we have plug-in defined in code, we have to fork the service provider for this specific project.
                    projectServiceProviderWithProject = projectServiceProviderWithProject.WithServices( additionalPlugIns.OfType<IService>() );
                }

                compilerPlugIns = additionalPlugIns
                    .Concat( this.ProjectOptions.PlugIns )
                    .ToImmutableArray();
            }
            else
            {
                compilerPlugIns = this.ProjectOptions.PlugIns;
            }

            // Creates a project model that includes the final service provider.
            var projectModel = new ProjectModel( compilation.Compilation, projectServiceProviderWithProject );

            // Create aspect types.
            var driverFactory = new AspectDriverFactory( compilation.Compilation, compilerPlugIns, projectServiceProviderWithProject );
            var aspectTypeFactory = new AspectClassMetadataFactory( projectServiceProviderWithProject, driverFactory );

            var aspectClasses = aspectTypeFactory.GetAspectClasses( compilation.Compilation, compileTimeProject, diagnosticAdder ).ToImmutableArray();

            // Get aspect parts and sort them.
            var unsortedAspectLayers = aspectClasses
                .Where( t => !t.IsAbstract )
                .SelectMany( at => at.Layers )
                .ToImmutableArray();

            var aspectOrderSources = new IAspectOrderingSource[]
            {
                new AttributeAspectOrderingSource( compilation.Compilation, loader ), new AspectLayerOrderingSource( aspectClasses )
            };

            if ( !AspectLayerSorter.TrySort( unsortedAspectLayers, aspectOrderSources, diagnosticAdder, out var orderedAspectLayers ) )
            {
                configuration = null;

                return false;
            }

            ImmutableArray<OrderedAspectLayer> allOrderedAspectLayers;
            BoundAspectClassCollection allAspectClasses;

            FabricsConfiguration? fabricsConfiguration;

            if ( compileTimeProject != null )
            {
                var fabricTopLevelAspectClass = new FabricTopLevelAspectClass( projectServiceProviderWithProject, roslynCompilation, compileTimeProject );
                var fabricAspectLayer = new OrderedAspectLayer( -1, fabricTopLevelAspectClass.Layer );

                allOrderedAspectLayers = orderedAspectLayers.Insert( 0, fabricAspectLayer );
                allAspectClasses = new BoundAspectClassCollection( aspectClasses.As<IBoundAspectClass>().Add( fabricTopLevelAspectClass ) );

                // Execute fabrics.
                var fabricManager = new FabricManager( allAspectClasses, this.ServiceProvider, compileTimeProject );
                fabricsConfiguration = fabricManager.ExecuteFabrics( compileTimeProject, roslynCompilation, projectModel, diagnosticAdder );
            }
            else
            {
                allOrderedAspectLayers = orderedAspectLayers;
                allAspectClasses = new BoundAspectClassCollection( aspectClasses.As<IBoundAspectClass>() );

                fabricsConfiguration = null;
            }

            // Freeze the project model to prevent any further modification of configuration.
            projectModel.Freeze();

            var stages = allOrderedAspectLayers
                .GroupAdjacent( x => GetGroupingKey( x.AspectClass.AspectDriver ) )
                .Select(
                    g =>
                        g.Key is _highLevelStageGroupingKey
                            ? new PipelineStageConfiguration( PipelineStageKind.HighLevel, g.ToImmutableArray(), null )
                            : new PipelineStageConfiguration( PipelineStageKind.LowLevel, g.ToImmutableArray(), (IAspectWeaver) g.Key ) )
                .ToImmutableArray();

            configuration = new AspectPipelineConfiguration(
                stages,
                allAspectClasses,
                allOrderedAspectLayers,
                compileTimeProject,
                loader,
                fabricsConfiguration,
                projectModel,
                projectServiceProviderWithProject );

            return true;

            static object GetGroupingKey( IAspectDriver driver )
                => driver switch
                {
                    // weavers are not grouped together
                    // Note: this requires that every AspectType has its own instance of IAspectWeaver
                    IAspectWeaver weaver => weaver,

                    // AspectDrivers are grouped together
                    AspectDriver => _highLevelStageGroupingKey,

                    _ => throw new AssertionFailedException()
                };
        }

        private protected virtual ImmutableArray<IAspectSource> CreateAspectSources(
            AspectPipelineConfiguration configuration,
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            var aspectClasses = configuration.AspectClasses.ToImmutableArray<IAspectClass>();

            var sources = ImmutableArray.Create<IAspectSource>(
                new CompilationAspectSource( aspectClasses, configuration.CompileTimeProjectLoader ),
                new ExternalInheritedAspectSource( compilation, aspectClasses, configuration.ServiceProvider, cancellationToken ) );

            if ( configuration.FabricsConfiguration != null )
            {
                sources = sources.AddRange( configuration.FabricsConfiguration.AspectSources );
            }

            return sources;
        }

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        private protected bool TryExecute(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? pipelineStageResult )
        {
            if ( pipelineConfiguration.CompileTimeProject == null || pipelineConfiguration.AspectClasses.Count == 0 )
            {
                // If there is no aspect in the compilation, don't execute the pipeline.
                pipelineStageResult = new PipelineStageResult( compilation, pipelineConfiguration.ProjectModel, ImmutableArray<OrderedAspectLayer>.Empty );

                return true;
            }

            var aspectSources = this.CreateAspectSources( pipelineConfiguration, compilation.Compilation, cancellationToken );

            pipelineStageResult = new PipelineStageResult(
                compilation,
                pipelineConfiguration.ProjectModel,
                pipelineConfiguration.AspectLayers,
                aspectSources: aspectSources );

            foreach ( var stageConfiguration in pipelineConfiguration.Stages )
            {
                var stage = this.CreateStage( stageConfiguration, pipelineConfiguration.CompileTimeProject );

                if ( stage == null )
                {
                    // This stage is skipped in the current pipeline (e.g. design-time).

                    continue;
                }

                if ( !stage.TryExecute( pipelineConfiguration, pipelineStageResult, diagnosticAdder, cancellationToken, out var newStageResult ) )
                {
                    return false;
                }
                else
                {
                    pipelineStageResult = newStageResult;
                }
            }

            var hasError = pipelineStageResult.Diagnostics.ReportedDiagnostics.Any( d => d.Severity >= DiagnosticSeverity.Error );

            foreach ( var diagnostic in pipelineStageResult.Diagnostics.ReportedDiagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();
                diagnosticAdder.Report( diagnostic );
            }

            return !hasError;
        }

        /// <summary>
        /// Creates an instance of <see cref="HighLevelPipelineStage"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="compileTimeProject"></param>
        /// <returns></returns>
        private protected abstract HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject );

        private protected virtual LowLevelPipelineStage? CreateLowLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
        {
            var partData = configuration.Parts.Single();

            return new LowLevelPipelineStage( configuration.Weaver!, partData.AspectClass, this.ServiceProvider );
        }

        private PipelineStage? CreateStage( PipelineStageConfiguration configuration, CompileTimeProject project )
        {
            switch ( configuration.Kind )
            {
                case PipelineStageKind.LowLevel:
                    return this.CreateLowLevelStage( configuration, project );

                case PipelineStageKind.HighLevel:

                    return this.CreateHighLevelStage( configuration, project );

                default:

                    throw new NotSupportedException();
            }
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( this._ownsDomain )
            {
                this._domain.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose() => this.Dispose( true );
    }
}