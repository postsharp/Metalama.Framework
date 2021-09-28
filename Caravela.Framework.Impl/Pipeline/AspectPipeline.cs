﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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

            this.ServiceProvider.AddService( projectOptions );
            this.ProjectOptions = projectOptions;

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
            var driverFactory = new AspectDriverFactory( this.ServiceProvider, compilation.Compilation, compilerPlugIns );
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

            if ( !AspectLayerSorter.TrySort( unsortedAspectLayers, aspectOrderSources, diagnosticAdder, out var sortedAspectLayers ) )
            {
                configuration = null;

                return false;
            }

            var aspectLayers = sortedAspectLayers.ToImmutableArray();

            var stages = aspectLayers
                .GroupAdjacent( x => GetGroupingKey( x.AspectClass.AspectDriver ) )
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), loader, compileTimeProject! ) )
                .ToImmutableArray();

            configuration = new AspectPipelineConfiguration( stages, aspectClasses, sortedAspectLayers, compileTimeProject, loader );

            return true;

            static object GetGroupingKey( IAspectDriver driver )
                => driver switch
                {
                    // weavers are not grouped together
                    // Note: this requires that every AspectType has its own instance of IAspectWeaver
                    IAspectWeaver weaver => weaver,

                    // AspectDrivers are grouped together
                    AspectDriver => nameof(AspectDriver),

                    _ => throw new NotSupportedException()
                };
        }

        private protected virtual ImmutableArray<IAspectSource> CreateAspectSources( AspectPipelineConfiguration configuration, Compilation compilation )
        {
            var sources = ImmutableArray.Create<IAspectSource>(
                new CompilationAspectSource( configuration.AspectClasses, configuration.CompileTimeProjectLoader ) );

            if ( configuration.CompileTimeProject != null )
            {
                var fabricContext = new FabricContext(
                    configuration.AspectClasses.ToImmutableDictionary( c => c.FullName, c => c ),
                    this.ServiceProvider,
                    configuration.CompileTimeProject );

                var fabricManager = new FabricManager( fabricContext );

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
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? pipelineStageResult )
        {
            var project = new ProjectModel( compilation.Compilation, this.ServiceProvider );

            if ( pipelineConfiguration.CompileTimeProject == null || pipelineConfiguration.AspectClasses.Count == 0 )
            {
                // If there is no aspect in the compilation, don't execute the pipeline.
                pipelineStageResult = new PipelineStageResult( compilation, project, Array.Empty<OrderedAspectLayer>() );

                return true;
            }

            var aspectSources = this.CreateAspectSources( pipelineConfiguration, compilation.Compilation );

            pipelineStageResult = new PipelineStageResult( compilation, project, pipelineConfiguration.Layers, aspectSources: aspectSources );

            foreach ( var stage in pipelineConfiguration.Stages )
            {
                if ( !stage.TryExecute( pipelineStageResult, diagnosticAdder, cancellationToken, out var newStageResult ) )
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
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader );

        private PipelineStage CreateStage(
            object groupKey,
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProjectLoader compileTimeProjectLoader,
            CompileTimeProject compileTimeProject )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new LowLevelPipelineStage( weaver, partData.AspectClass, this.ServiceProvider );

                case nameof(AspectDriver):

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