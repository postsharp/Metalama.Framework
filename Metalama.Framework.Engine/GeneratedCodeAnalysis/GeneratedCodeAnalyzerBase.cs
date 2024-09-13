// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Metalama.Framework.Diagnostics.Severity;

namespace Metalama.Framework.Engine.GeneratedCodeAnalysis;

#pragma warning disable RS1022 // Remove access to our implementation types 
#pragma warning disable RS1026 // Enable concurrent execution

internal abstract class GeneratedCodeAnalyzerBase : DiagnosticAnalyzer
{
    private const string _diagnosticCategory = "Metalama.GeneratedCodeAnalyzer";

    internal static readonly DiagnosticDefinition<(string AspectType, IRef<IDeclaration> Target, string Addendum)> AspectAppliedToGeneratedCode = new(
        "LAMA0320",
        "Aspect can't be applied to source generated code.",
        "The aspect '{0}' can't be applied to '{1}', because it's in source generated code.{2}",
        _diagnosticCategory,
        Warning );

    [MemberNotNull( nameof(_logger), nameof(_serviceProvider) )]
    private bool IsEnabled { get; set; }

    private ILogger? _logger;
    private GlobalServiceProvider? _serviceProvider;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create( AspectAppliedToGeneratedCode.ToRoslynDescriptor() );

    public override void Initialize( AnalysisContext context )
    {
        if ( !BackstageServiceFactory.IsInitialized )
        {
            // At compile-time, the transformer must have run before. At design-time, design-time types must have been used before.
            // If somehow neither happened, this analyzer won't try to run.
            return;
        }

        var serviceProvider = ServiceProviderFactory.GetServiceProvider();

        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "GeneratedCodeAnalyzer" );
        this._serviceProvider = serviceProvider;

        this.IsEnabled = true;

        context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics );
    }

    protected GeneratedCodeAnalyzerPipeline CreatePipeline( Compilation compilation, AnalyzerOptions options )
    {
        var globalServices = this._serviceProvider!.Value.Underlying;

        var projectOptionsFactory = globalServices.GetRequiredService<IProjectOptionsFactory>();
        var projectOptions = projectOptionsFactory.GetProjectOptions( options.AnalyzerConfigOptionsProvider );

        var serviceProvider = globalServices.WithProjectScopedServices( projectOptions, compilation );

        return new GeneratedCodeAnalyzerPipeline( serviceProvider );
    }

    protected AspectPipelineConfiguration? TryInitializePipeline(
        GeneratedCodeAnalyzerPipeline pipeline,
        Compilation compilation,
        CancellationToken cancellationToken )
    {
        if ( !this.IsEnabled )
        {
            return null;
        }

        var initializeResult = pipeline.Initialize( compilation, cancellationToken );

        if ( !initializeResult.IsSuccessful )
        {
            this._logger.Error?.Log( $"Failed to initialize pipeline: {initializeResult.Diagnostics}" );

            return null;
        }

        return initializeResult.Value;
    }

    protected void AnalyzeTrees(
        GeneratedCodeAnalyzerPipeline pipeline,
        AspectPipelineConfiguration configuration,
        Action<Diagnostic> reportDiagnostic,
        Compilation compilation,
        IReadOnlyList<SyntaxTree> trees,
        CancellationToken cancellationToken )
    {
        if ( !this.IsEnabled )
        {
            return;
        }

        var partialCompilation = PartialCompilation.CreatePartial( compilation, trees );

        var taskRunner = pipeline.ServiceProvider.Global.GetRequiredService<ITaskRunner>();

        var aspectInstancesResult =
            taskRunner.RunSynchronously( () => pipeline.ComputeAspectInstancesAsync( partialCompilation, configuration, cancellationToken ), cancellationToken );

        if ( !aspectInstancesResult.IsSuccessful )
        {
            this._logger.Error?.Log( $"Failed to get aspect instances: {aspectInstancesResult.Diagnostics}" );

            return;
        }

        var compilationContext = compilation.GetCompilationContext();

        foreach ( var aspectInstance in aspectInstancesResult.Value )
        {
            var location = aspectInstance.Predecessors is [{ Instance: IAttribute attribute }]
                ? attribute.GetDiagnosticLocation()
                : aspectInstance.TargetDeclaration.GetClosestSymbol( compilationContext ).GetDiagnosticLocation();

            if ( trees.Contains( location?.SourceTree ) )
            {
                var addendum = string.Empty;

                if ( location != null && Path.GetExtension( location.GetMappedLineSpan().Path ) is ".razor" or ".cshtml" )
                {
                    addendum = " For Razor files, consider extracting the relevant code to code behind.";
                }

                var diagnostic = AspectAppliedToGeneratedCode.CreateRoslynDiagnostic(
                    location,
                    (aspectInstance.AspectClass.ShortName, aspectInstance.TargetDeclaration, addendum) );

                reportDiagnostic( diagnostic );
            }
        }
    }

    protected sealed class GeneratedCodeAnalyzerPipeline( ServiceProvider<IProjectService> serviceProvider )
        : AspectPipeline( serviceProvider, ExecutionScenario.GeneratedCodeAnalyzer, domain: null )
    {
        public FallibleResultWithDiagnostics<AspectPipelineConfiguration> Initialize( Compilation compilation, CancellationToken cancellationToken )
        {
            var diagnosticBag = new DiagnosticBag();

            if ( !this.TryInitialize( diagnosticBag, compilation, projectLicenseInfo: null, compileTimeTreesHint: null, cancellationToken, out var configuration ) )
            {
                return FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Failed( diagnosticBag.ToImmutableArray() );
            }

            return configuration;
        }

        public async Task<FallibleResultWithDiagnostics<IReadOnlyList<IAspectInstance>>> ComputeAspectInstancesAsync(
            PartialCompilation partialCompilation,
            AspectPipelineConfiguration configuration,
            CancellationToken cancellationToken )
        {
            var diagnosticBag = new DiagnosticBag();

            var result = await this.ExecuteAsync( partialCompilation, diagnosticBag, configuration, cancellationToken.ToTestable() );

            if ( !result.IsSuccessful )
            {
                return FallibleResultWithDiagnostics<IReadOnlyList<IAspectInstance>>.Failed( diagnosticBag.ToImmutableArray() );
            }

            return FallibleResultWithDiagnostics<IReadOnlyList<IAspectInstance>>.Succeeded( result.Value.AspectInstances );
        }

        protected override SyntaxGenerationOptions GetSyntaxGenerationOptions() => SyntaxGenerationOptions.Unformatted;
    }
}