// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
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
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// The base class for the main process of Metalama.
    /// </summary>
    public abstract class AspectPipeline : IDisposable
    {
        private const string _highLevelStageGroupingKey = nameof(_highLevelStageGroupingKey);

        private static readonly ImmutableHashSet<LanguageVersion> _supportedVersions = ImmutableHashSet.Create(
            LanguageVersion.Latest,
            LanguageVersion.LatestMajor,
            LanguageVersion.CSharp10 );

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
            ExecutionScenario executionScenario,
            bool isTest,
            CompileTimeDomain? domain )
        {
            this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "AspectPipeline" );

            this.ProjectOptions = serviceProvider.GetRequiredService<IProjectOptions>();

            this.ServiceProvider = serviceProvider.WithServices( this.ProjectOptions.PlugIns.OfType<IService>() );

            if ( isTest )
            {
                // We use a single-threaded task scheduler for tests because the test runner itself is already multi-threaded and
                // most tests are so small that they do not allow for significant concurrency anyway. A specific test can provide a different scheduler.
                // We randomize the ordering of execution to improve the test relevance.

                if ( serviceProvider.GetService<ITaskScheduler>() == null )
                {
                    this.ServiceProvider = this.ServiceProvider.WithService( new RandomizingSingleThreadedTaskScheduler( serviceProvider ) );
                }

                this.ServiceProvider = this.ServiceProvider
                    .WithServices( executionScenario.WithTest() )
                    .WithService( new TestMarkerService() );
            }
            else
            {
                this.ServiceProvider = this.ServiceProvider
                    .WithService( this.ProjectOptions.IsConcurrentBuildEnabled ? new ConcurrentTaskScheduler() : new SingleThreadedTaskScheduler() )
                    .WithServices( executionScenario );
            }

            this.ServiceProvider = this.ServiceProvider.WithMark( ServiceProviderMark.Pipeline );

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
            ProjectLicenseInfo? projectLicenseInfo,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            this.PipelineInitializationCount++;

            var roslynCompilation = compilation.Compilation;

            // Check language version.

            var languageVersion =
                (((CSharpParseOptions?) compilation.Compilation.SyntaxTrees.FirstOrDefault()?.Options)?.LanguageVersion ?? LanguageVersion.Latest)
                .MapSpecifiedToEffectiveVersion();

            if ( languageVersion == LanguageVersion.Preview )
            {
                if ( !this.ProjectOptions.AllowPreviewLanguageFeatures )
                {
                    diagnosticAdder.Report( GeneralDiagnosticDescriptors.PreviewCSharpVersionNotSupported.CreateRoslynDiagnostic( null, default ) );
                    configuration = null;

                    return false;
                }
            }
            else if ( !_supportedVersions.Contains( languageVersion ) )
            {
                diagnosticAdder.Report(
                    GeneralDiagnosticDescriptors.CSharpVersionNotSupported.CreateRoslynDiagnostic(
                        null,
                        (languageVersion.ToDisplayString(), _supportedVersions.Select( x => x.ToDisplayString() ).ToArray()) ) );

                configuration = null;

                return false;
            }

            // Check the Metalama version.
            var referencedMetalamaVersions = compilation.Compilation.SourceModule.ReferencedAssemblies
                .Where( identity => identity.Name == "Metalama.Framework" )
                .Select( x => x.Version )
                .ToList();

            if ( referencedMetalamaVersions.Count != 1 || referencedMetalamaVersions[0] != EngineAssemblyMetadataReader.Instance.AssemblyVersion )
            {
                diagnosticAdder.Report(
                    GeneralDiagnosticDescriptors.MetalamaVersionNotSupported.CreateRoslynDiagnostic(
                        null,
                        (referencedMetalamaVersions.Select( x => x.ToString() ).ToArray(),
                         EngineAssemblyMetadataReader.Instance.AssemblyVersion.ToString()) ) );

                configuration = null;

                return false;
            }

            // Create dependencies.

            var loader = CompileTimeProjectLoader.Create( this.Domain, this.ServiceProvider );

            // Prepare the compile-time assembly.
            if ( !loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    projectLicenseInfo,
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
            
            // Initialize the licensing service with redistribution licenses.
            // Add the license verifier.
            var licenseConsumptionManager = projectServiceProviderWithProject.GetService<ILicenseConsumptionManagerProvider>()?.LicenseConsumptionManager;

            if ( licenseConsumptionManager != null )
            {
                var licenseVerifier = new LicenseVerifier( licenseConsumptionManager, compilation.Compilation.AssemblyName );

                if ( !licenseVerifier.TryInitialize( compileTimeProject, diagnosticAdder ) )
                {
                    configuration = null;
                    
                    return false;
                }
                
                projectServiceProviderWithProject = projectServiceProviderWithProject.WithService( licenseVerifier );
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
                var fabricAspectLayer = new OrderedAspectLayer( -1, -1, fabricTopLevelAspectClass.Layer );

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
        protected Task<FallibleResult<AspectPipelineResult>> ExecuteAsync(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration? pipelineConfiguration,
            CancellationToken cancellationToken )
            => this.ExecuteAsync( compilation, null, diagnosticAdder, pipelineConfiguration, cancellationToken );

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="AspectPipelineResult"/>.
        /// </summary>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        protected Task<FallibleResult<AspectPipelineResult>> ExecuteAsync(
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration? pipelineConfiguration,
            CancellationToken cancellationToken )
            => this.ExecuteAsync(
                compilation.PartialCompilation,
                compilation,
                diagnosticAdder,
                pipelineConfiguration,
                cancellationToken );

        private async Task<FallibleResult<AspectPipelineResult>> ExecuteAsync(
            PartialCompilation compilation,
            CompilationModel? compilationModel,
            IDiagnosticAdder diagnosticAdder,
            AspectPipelineConfiguration? pipelineConfiguration,
            CancellationToken cancellationToken )
        {
            if ( pipelineConfiguration == null )
            {
                if ( !this.TryInitialize( diagnosticAdder, compilation, null, null, cancellationToken, out pipelineConfiguration ) )
                {
                    return default;
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
                return new AspectPipelineResult(
                    compilation,
                    pipelineConfiguration.ProjectModel,
                    ImmutableArray<OrderedAspectLayer>.Empty,
                    ImmutableArray<CompilationModel>.Empty );
            }

            var aspectSources = this.CreateAspectSources( pipelineConfiguration, compilation.Compilation, cancellationToken );
            var additionalCompilationOutputFiles = GetAdditionalCompilationOutputFiles( pipelineConfiguration.ServiceProvider );

            // Execute the pipeline stages.
            var pipelineStageResult = new AspectPipelineResult(
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

                var stageResult = await stage.ExecuteAsync( pipelineConfiguration, pipelineStageResult, diagnosticAdder, cancellationToken );

                if ( !stageResult.IsSuccess )
                {
                    return default;
                }
                else
                {
                    pipelineStageResult = stageResult.Value;
                }
            }

            // Enforce licensing.
            var licenseVerifier = pipelineConfiguration.ServiceProvider.GetService<LicenseVerifier>();

            if ( licenseVerifier != null )
            {
                var compileTimeProject = pipelineConfiguration.ServiceProvider.GetRequiredService<CompileTimeProject>();
                var licensingDiagnostics = new UserDiagnosticSink( compileTimeProject );
                licenseVerifier.VerifyCompilationResult( compilation.Compilation, pipelineStageResult.AspectInstanceResults, licensingDiagnostics );
                pipelineStageResult = pipelineStageResult.WithAdditionalDiagnostics( licensingDiagnostics.ToImmutable() );
            }

            // Report diagnostics
            foreach ( var diagnostic in pipelineStageResult.Diagnostics.ReportedDiagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();
                diagnosticAdder.Report( diagnostic );
            }

            return FallibleResult<AspectPipelineResult>.Succeeded( pipelineStageResult );
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

        private protected virtual LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration )
        {
            var partData = configuration.AspectLayers.Single();

            return new LowLevelPipelineStage( configuration.Weaver!, partData.AspectClass, this.ServiceProvider );
        }

        private PipelineStage? CreateStage( PipelineStageConfiguration configuration, CompileTimeProject project )
        {
            switch ( configuration.Kind )
            {
                case PipelineStageKind.LowLevel:
                    return this.CreateLowLevelStage( configuration );

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