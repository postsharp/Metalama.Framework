// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Fabrics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Impl.ServiceProvider;
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

        public ServiceProvider.ServiceProvider ServiceProvider { get; }

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
            IProjectOptions projectOptions,
            AspectExecutionScenario executionScenario,
            bool isTest,
            CompileTimeDomain? domain,
            IPathOptions? directoryOptions = null,
            IAssemblyLocator? assemblyLocator = null )
        {
            this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( directoryOptions, assemblyLocator );

            var existingProjectOptions = this.ServiceProvider.GetOptionalService<IProjectOptions>();

            if ( existingProjectOptions != null )
            {
                // TryCaravela uses this scenario to define options.
                projectOptions = existingProjectOptions.Apply( projectOptions );
            }

            // Register project options.
            this.ServiceProvider.AddService( projectOptions );
            this.ProjectOptions = projectOptions;

            // Register plug ins to the Service Provider.
            foreach ( var plugIn in projectOptions.PlugIns )
            {
                if ( plugIn is IService service )
                {
                    this.ServiceProvider.AddService( service );
                }
            }

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

            // ReSharper disable once VirtualMemberCallInConstructor
            this.ServiceProvider.AddService( new AspectPipelineDescription( executionScenario, isTest ) );
        }

        internal int PipelineInitializationCount { get; set; }

        private protected bool TryInitialize(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectProjectConfiguration? configuration )
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

            // Create compiler plug-ins found in compile-time code.
            ImmutableArray<object> compilerPlugIns;

            if ( compileTimeProject != null )
            {
                // The instantiation of compiler plug-ins defined in the current compilation is a bit rough here, but it is supposed to be used
                // by our internal tests only.

                var invoker = this.ServiceProvider.GetService<UserCodeInvoker>();

                compilerPlugIns = compileTimeProject.ClosureProjects
                    .SelectMany( p => p.CompilerPlugInTypes.Select( t => invoker.Invoke( () => Activator.CreateInstance( p.GetType( t ) ) ) ) )
                    .Concat( this.ProjectOptions.PlugIns )
                    .ToImmutableArray();
            }
            else
            {
                compilerPlugIns = this.ProjectOptions.PlugIns;
            }

            // Create aspect types.
            var driverFactory = new AspectDriverFactory( compilation.Compilation, compilerPlugIns, this.ServiceProvider );
            var aspectTypeFactory = new AspectClassMetadataFactory( this.ServiceProvider, driverFactory );

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
            ImmutableArray<IBoundAspectClass> allAspectClasses;

            if ( compileTimeProject != null )
            {
                var fabricTopLevelAspectClass = new FabricTopLevelAspectClass( this.ServiceProvider, roslynCompilation, compileTimeProject );
                var fabricAspectLayer = new OrderedAspectLayer( -1, fabricTopLevelAspectClass.Layer );

                allOrderedAspectLayers = orderedAspectLayers.Insert( 0, fabricAspectLayer );
                allAspectClasses = aspectClasses.As<IBoundAspectClass>().Add( fabricTopLevelAspectClass );
            }
            else
            {
                allOrderedAspectLayers = orderedAspectLayers;
                allAspectClasses = aspectClasses.As<IBoundAspectClass>();
            }

            var stages = allOrderedAspectLayers
                .GroupAdjacent( x => GetGroupingKey( x.AspectClass.AspectDriver ) )
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), loader, compileTimeProject! ) )
                .ToImmutableArray();

            configuration = new AspectProjectConfiguration(
                stages,
                allAspectClasses,
                allOrderedAspectLayers,
                compileTimeProject,
                loader,
                this.ServiceProvider );

            return true;

            static object GetGroupingKey( IAspectDriver driver )
                => driver switch
                {
                    // weavers are not grouped together
                    // Note: this requires that every AspectType has its own instance of IAspectWeaver
                    IAspectWeaver weaver => weaver,

                    // AspectDrivers are grouped together
                    IHighLevelAspectDriver => _highLevelStageGroupingKey,

                    _ => throw new AssertionFailedException()
                };
        }

        private protected virtual ImmutableArray<IAspectSource> CreateAspectSources(
            AspectProjectConfiguration configuration,
            Compilation compilation )
        {
            var sources = ImmutableArray.Create<IAspectSource>(
                new CompilationAspectSource( configuration.AspectClasses.As<IAspectClass>(), configuration.CompileTimeProjectLoader ) );

            if ( configuration.CompileTimeProject != null )
            {
                var fabricManager = new FabricManager( configuration );

                fabricManager.ExecuteFabrics( configuration.CompileTimeProject, compilation );

                sources = sources.Add( fabricManager.AspectSource );
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
            AspectProjectConfiguration projectConfiguration,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? pipelineStageResult )
        {
            var project = new ProjectModel( compilation.Compilation, this.ServiceProvider );

            if ( projectConfiguration.CompileTimeProject == null || projectConfiguration.AspectClasses.Length == 0 )
            {
                // If there is no aspect in the compilation, don't execute the pipeline.
                pipelineStageResult = new PipelineStageResult( compilation, project, Array.Empty<OrderedAspectLayer>() );

                return true;
            }

            var aspectSources = this.CreateAspectSources( projectConfiguration, compilation.Compilation );

            pipelineStageResult = new PipelineStageResult( compilation, project, projectConfiguration.AspectLayers, aspectSources: aspectSources );

            foreach ( var stage in projectConfiguration.Stages )
            {
                if ( !stage.TryExecute( projectConfiguration, pipelineStageResult, diagnosticAdder, cancellationToken, out var newStageResult ) )
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
        /// <param name="parts"></param>
        /// <param name="compileTimeProject"></param>
        /// <param name="compileTimeProjectLoader"></param>
        /// <returns></returns>
        private protected abstract HighLevelPipelineStage CreateStage(
            ImmutableArray<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader );

        private PipelineStage CreateStage(
            object groupKey,
            ImmutableArray<OrderedAspectLayer> parts,
            CompileTimeProjectLoader compileTimeProjectLoader,
            CompileTimeProject compileTimeProject )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new LowLevelPipelineStage( weaver, partData.AspectClass, this.ServiceProvider );

                case _highLevelStageGroupingKey:

                    return this.CreateStage( parts, compileTimeProject, compileTimeProjectLoader );

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