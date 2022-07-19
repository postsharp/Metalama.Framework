﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// The base class for the main process of Metalama.
    /// </summary>
    public abstract class AspectPipeline : IDisposable
    {
        private const string _highLevelStageGroupingKey = nameof(_highLevelStageGroupingKey);
        private readonly bool _ownsDomain;

        public IProjectOptions ProjectOptions { get; }

        protected CompileTimeDomain Domain { get; }

        // This member is intentionally protected because there can be one ServiceProvider per project,
        // but the pipeline can be used by many projects.
        public ServiceProvider ServiceProvider { get; }

        protected ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectPipeline"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="executionScenario"></param>
        /// <param name="isTest"></param>
        /// <param name="domain">If <c>null</c>, the instance is created from the <see cref="ICompileTimeDomainFactory"/> service.</param>
        protected AspectPipeline(
            ServiceProvider serviceProvider,
            IExecutionScenario executionScenario,
            bool isTest,
            CompileTimeDomain? domain )
        {
            this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "AspectPipeline" );

            this.ProjectOptions = serviceProvider.GetRequiredService<IProjectOptions>();

            this.ServiceProvider = serviceProvider
                .WithServices( this.ProjectOptions.PlugIns.OfType<IService>() )
                .WithServices( new AspectPipelineDescription( executionScenario, isTest ) )
                .WithMark( ServiceProviderMark.Pipeline );

            if ( domain != null )
            {
                this.Domain = domain;
            }
            else
            {
                // Coverage: Ignore (tests always provide a domain).
                this.Domain = this.ServiceProvider.GetRequiredService<ICompileTimeDomainFactory>().CreateDomain();
                this._ownsDomain = true;
            }
        }

        internal int PipelineInitializationCount { get; private set; }

        protected bool TryInitialize(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            this.PipelineInitializationCount++;

            var roslynCompilation = compilation.Compilation;
            
            // Check language version.
            if ( compilation.SyntaxTrees.Count > 0 && ((CSharpParseOptions) compilation.SyntaxTrees.First().Value.Options).LanguageVersion == LanguageVersion.Preview )
            {
                diagnosticAdder.Report( GeneralDiagnosticDescriptors.PreviewCSharpVersionNotSupported.CreateRoslynDiagnostic( null, default ) );
                configuration = null;

                return false;
            }

            // Create dependencies.

            var loader = CompileTimeProjectLoader.Create( this.Domain, this.ServiceProvider );

            // Prepare the compile-time assembly.
            if ( !loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    compileTimeTreesHint,
                    diagnosticAdder,
                    false,
                    cancellationToken,
                    out var compileTimeProject ) )
            {
                this.Logger.Warning?.Log( $"TryInitialized({this.ProjectOptions.AssemblyName}) failed: cannot get the compile-time compilation." );

                configuration = null;

                return false;
            }

            // Create a project-level service provider.
            var projectServiceProviderWithoutPlugins = this.ServiceProvider.WithService( loader )
                .WithMark( ServiceProviderMark.Project );

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

                var invoker = this.ServiceProvider.GetRequiredService<UserCodeInvoker>();

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
                                diagnosticAdder.Report(
                                    GeneralDiagnosticDescriptors.TypeMustHavePublicDefaultConstructor.CreateRoslynDiagnostic( null, type ) );

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

            // Create a compilation model for the aspect initialization.
            var compilationModel = CompilationModel.CreateInitialInstance( projectModel, compilation );

            // Create aspect types.
            // We create a TemplateAttributeFactory for this purpose but we cannot add it to the ServiceProvider that will flow out because
            // we don't want to leak the compilation for the design-time scenario.
            var serviceProviderForAspectClassFactory =
                projectServiceProviderWithProject.WithService( new TemplateAttributeFactory( projectServiceProviderWithProject, roslynCompilation ) );

            var driverFactory = new AspectDriverFactory( compilationModel, compilerPlugIns, serviceProviderForAspectClassFactory );
            var aspectTypeFactory = new AspectClassFactory( serviceProviderForAspectClassFactory, driverFactory );

            var aspectClasses = aspectTypeFactory.GetClasses( compilation.Compilation, compileTimeProject, diagnosticAdder ).ToImmutableArray();

            // Get aspect parts and sort them.
            var unsortedAspectLayers = aspectClasses
                .Where( t => !t.IsAbstract )
                .SelectMany( at => at.Layers )
                .ToImmutableArray();

            var aspectOrderSources = new IAspectOrderingSource[]
            {
                new AttributeAspectOrderingSource( compilation.Compilation, loader ),
                new AspectLayerOrderingSource( aspectClasses ),
                new FrameworkAspectOrderingSource( aspectClasses )
            };

            if ( !AspectLayerSorter.TrySort( unsortedAspectLayers, aspectOrderSources, diagnosticAdder, out var orderedAspectLayers ) )
            {
                this.Logger.Warning?.Log( $"TryInitialized({this.ProjectOptions.AssemblyName}) failed: cannot sort aspect layers." );

                configuration = null;

                return false;
            }

            // Create other template classes.
            var otherTemplateClassFactory = new OtherTemplateClassFactory( serviceProviderForAspectClassFactory );

            var otherTemplateClasses = otherTemplateClassFactory.GetClasses( compilation.Compilation, compileTimeProject, diagnosticAdder )
                .ToImmutableDictionary( x => x.FullName, x => x );

            // Add fabrics.
            ImmutableArray<OrderedAspectLayer> allOrderedAspectLayers;
            BoundAspectClassCollection allAspectClasses;

            FabricsConfiguration? fabricsConfiguration;

            if ( compileTimeProject != null )
            {
                var fabricTopLevelAspectClass = new FabricTopLevelAspectClass( projectServiceProviderWithProject, compilationModel, compileTimeProject );
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
                this.Domain,
                stages,
                allAspectClasses,
                otherTemplateClasses,
                allOrderedAspectLayers,
                compileTimeProject,
                loader,
                fabricsConfiguration,
                projectModel,
                projectServiceProviderWithProject,
                this.FilterCodeFix );

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

        private protected virtual bool FilterCodeFix( IDiagnosticDefinition diagnosticDefinition, Location location ) => false;

        private protected virtual ( ImmutableArray<IAspectSource> AspectSources, ImmutableArray<IValidatorSource> ValidatorSources) CreateAspectSources(
            AspectPipelineConfiguration configuration,
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            var aspectClasses = configuration.BoundAspectClasses.ToImmutableArray<IAspectClass>();

            var transitiveAspectSource = new TransitiveAspectSource( compilation, aspectClasses, configuration.ServiceProvider, cancellationToken );

            var aspectSources = ImmutableArray.Create<IAspectSource>(
                new CompilationAspectSource( aspectClasses, configuration.CompileTimeProjectLoader ),
                transitiveAspectSource );

            var validatorSources = ImmutableArray.Create<IValidatorSource>( transitiveAspectSource );

            if ( configuration.FabricsConfiguration != null )
            {
                aspectSources = aspectSources.AddRange( configuration.FabricsConfiguration.AspectSources );
                validatorSources = validatorSources.AddRange( configuration.FabricsConfiguration.ValidatorSources );
            }

            return (aspectSources, validatorSources);
        }

        private static ImmutableArray<AdditionalCompilationOutputFile> GetAdditionalCompilationOutputFiles( ServiceProvider serviceProvider )
        {
            var provider = serviceProvider.GetService<IAdditionalOutputFileProvider>();

            if ( provider == null )
            {
                return ImmutableArray<AdditionalCompilationOutputFile>.Empty;
            }

            return provider.GetAdditionalCompilationOutputFiles();
        }

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="AspectPipelineResult"/>.
        /// </summary>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        public bool TryExecute(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration? pipelineConfiguration,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineResult? pipelineStageResult )
            => this.TryExecute( compilation, null, diagnosticAdder, pipelineConfiguration, cancellationToken, out pipelineStageResult );

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="AspectPipelineResult"/>.
        /// </summary>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        public bool TryExecute(
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration? pipelineConfiguration,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineResult? pipelineStageResult )
            => this.TryExecute(
                compilation.PartialCompilation,
                compilation,
                diagnosticAdder,
                pipelineConfiguration,
                cancellationToken,
                out pipelineStageResult );

        private bool TryExecute(
            PartialCompilation compilation,
            CompilationModel? compilationModel,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration? pipelineConfiguration,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineResult? pipelineStageResult )
        {
            if ( pipelineConfiguration == null )
            {
                if ( !this.TryInitialize( diagnosticAdder, compilation, null, cancellationToken, out pipelineConfiguration ) )
                {
                    pipelineStageResult = null;

                    return false;
                }
            }

            // Add services that have a reference to the compilation.
            pipelineConfiguration =
                pipelineConfiguration.WithServiceProvider(
                    pipelineConfiguration.ServiceProvider
                        .WithService( new TemplateAttributeFactory( pipelineConfiguration.ServiceProvider, compilation.Compilation ) )
                        .WithService( new AttributeClassificationService() ) );

            // When we reuse a pipeline configuration created from a different pipeline (e.g. design-time to code fix),
            // we need to substitute the code fix filter.
            pipelineConfiguration = pipelineConfiguration.WithCodeFixFilter( this.FilterCodeFix );

            if ( pipelineConfiguration.CompileTimeProject == null || pipelineConfiguration.BoundAspectClasses.Count == 0 )
            {
                // If there is no aspect in the compilation, don't execute the pipeline.
                pipelineStageResult = new AspectPipelineResult(
                    compilation,
                    pipelineConfiguration.ProjectModel,
                    ImmutableArray<OrderedAspectLayer>.Empty,
                    ImmutableArray<CompilationModel>.Empty );

                return true;
            }

            var aspectSources = this.CreateAspectSources( pipelineConfiguration, compilation.Compilation, cancellationToken );
            var additionalCompilationOutputFiles = GetAdditionalCompilationOutputFiles( pipelineConfiguration.ServiceProvider );

            pipelineStageResult = new AspectPipelineResult(
                compilation,
                pipelineConfiguration.ProjectModel,
                pipelineConfiguration.AspectLayers,
                compilationModel == null ? ImmutableArray<CompilationModel>.Empty : ImmutableArray.Create( compilationModel ),
                null,
                aspectSources.AspectSources,
                aspectSources.ValidatorSources,
                additionalCompilationOutputFiles: additionalCompilationOutputFiles );

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
            var partData = configuration.AspectLayers.Single();

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
                this.Domain.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose() => this.Dispose( true );
    }
}