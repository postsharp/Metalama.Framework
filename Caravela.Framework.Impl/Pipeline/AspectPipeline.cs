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
            var projectServiceProvider = this.ServiceProvider.WithService( loader ).WithMark( ServiceProviderMark.Project );

            if ( compileTimeProject != null )
            {
                // The instantiation of compiler plug-ins defined in the current compilation is a bit rough here, but it is supposed to be used
                // by our internal tests only. However, the logic will interfere with production scenario, where a plug-in will be both
                // in ProjectOptions.PlugIns and in CompileTimeProjects.PlugInTypes. So, we do not load the plug ins found by CompileTimeProjects.PlugInTypes
                // if they are already provided by ProjectOptions.PlugIns.

                var invoker = this.ServiceProvider.GetService<UserCodeInvoker>();

                var loadedPlugInsTypes = this.ProjectOptions.PlugIns.Select( t => t.GetType().FullName ).ToImmutableArray();

                var additionalPlugIns = compileTimeProject.ClosureProjects
                    .SelectMany( p => p.PlugInTypes.Select( t => (Project: p, TypeName: t) ) )
                    .Where( t => !loadedPlugInsTypes.Contains( t.TypeName ) )
                    .Select( t => invoker.Invoke( () => Activator.CreateInstance( t.Project.GetType( t.TypeName ) ) ) )
                    .ToList();

                if ( additionalPlugIns.Count > 0 )
                {
                    // If we have plug-in defined in code, we have to fork the service provider for this specific project.
                    projectServiceProvider = projectServiceProvider.WithServices( additionalPlugIns.OfType<IService>() );
                }

                compilerPlugIns = additionalPlugIns
                    .Concat( this.ProjectOptions.PlugIns )
                    .ToImmutableArray();
            }
            else
            {
                compilerPlugIns = this.ProjectOptions.PlugIns;
            }

            // Create aspect types.
            var driverFactory = new AspectDriverFactory( compilation.Compilation, compilerPlugIns, projectServiceProvider );
            var aspectTypeFactory = new AspectClassMetadataFactory( projectServiceProvider, driverFactory );

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
                var fabricTopLevelAspectClass = new FabricTopLevelAspectClass( projectServiceProvider, roslynCompilation, compileTimeProject );
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
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), compileTimeProject! ) )
                .ToImmutableArray();

            configuration = new AspectProjectConfiguration(
                stages,
                allAspectClasses,
                allOrderedAspectLayers,
                compileTimeProject,
                loader,
                projectServiceProvider );

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
            AspectProjectConfiguration configuration,
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            var aspectClasses = configuration.AspectClasses.As<IAspectClass>();

            var sources = ImmutableArray.Create<IAspectSource>(
                new CompilationAspectSource( aspectClasses, configuration.CompileTimeProjectLoader ),
                new ExternalInheritedAspectSource( compilation, aspectClasses, configuration.ServiceProvider, cancellationToken ) );

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
            var project = new ProjectModel( compilation.Compilation, projectConfiguration.ServiceProvider );

            if ( projectConfiguration.CompileTimeProject == null || projectConfiguration.AspectClasses.Length == 0 )
            {
                // If there is no aspect in the compilation, don't execute the pipeline.
                pipelineStageResult = new PipelineStageResult( compilation, project, ImmutableArray<OrderedAspectLayer>.Empty );

                return true;
            }

            var aspectSources = this.CreateAspectSources( projectConfiguration, compilation.Compilation, cancellationToken );

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
        /// <returns></returns>
        private protected abstract HighLevelPipelineStage CreateStage(
            ImmutableArray<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject );

        private PipelineStage CreateStage(
            object groupKey,
            ImmutableArray<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new LowLevelPipelineStage( weaver, partData.AspectClass, this.ServiceProvider );

                case _highLevelStageGroupingKey:

                    return this.CreateStage( parts, compileTimeProject );

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