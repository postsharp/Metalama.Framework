// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Compiler;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.CompileTime
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used at compile time.
    /// </summary>
    public class CompileTimeAspectPipeline : AspectPipeline
    {
        public CompileTimeAspectPipeline(
            ServiceProvider serviceProvider,
            bool isTest,
            CompileTimeDomain? domain = null,
            ExecutionScenario? executionScenario = null ) : base(
            serviceProvider,
            executionScenario ?? ExecutionScenario.CompileTime,
            isTest,
            domain ) { }

        public async Task<FallibleResult<CompileTimeAspectPipelineResult>> ExecuteAsync(
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            ImmutableArray<ManagedResource> resources,
            CancellationToken cancellationToken )
        {
            var partialCompilation = PartialCompilation.CreateComplete( compilation );

            // Skip if Metalama has been disabled for this project.
            if ( !this.ProjectOptions.IsFrameworkEnabled )
            {
                return new CompileTimeAspectPipelineResult(
                    ImmutableArray<SyntaxTreeTransformation>.Empty,
                    ImmutableArray<ManagedResource>.Empty,
                    partialCompilation,
                    ImmutableArray<AdditionalCompilationOutputFile>.Empty );
            }

            // Report error if the compilation does not have the METALAMA preprocessor symbol.
            if ( !(compilation.SyntaxTrees.FirstOrDefault()?.Options.PreprocessorSymbolNames.Contains( "METALAMA" ) ?? false) )
            {
                diagnosticAdder.Report( GeneralDiagnosticDescriptors.MissingMetalamaPreprocessorSymbol.CreateRoslynDiagnosticImpl( null, null ) );

                return default;
            }

            // Validate the code (some validations are not done by the template compiler).
            var isTemplatingCodeValidatorSuccessful = await TemplatingCodeValidator.ValidateAsync(
                compilation,
                diagnosticAdder,
                this.ServiceProvider,
                cancellationToken );

            if ( !isTemplatingCodeValidatorSuccessful )
            {
                return default;
            }

            var licenseConsumptionManager = this.ServiceProvider.GetBackstageService<ILicenseConsumptionManager>();
            var redistributionLicenseKey = licenseConsumptionManager?.RedistributionLicenseKey;

            var projectLicenseInfo = string.IsNullOrEmpty( redistributionLicenseKey )
                ? ProjectLicenseInfo.Empty
                : new ProjectLicenseInfo( redistributionLicenseKey );

            // Initialize the pipeline and generate the compile-time project.
            if ( !this.TryInitialize( diagnosticAdder, partialCompilation, projectLicenseInfo, null, cancellationToken, out var configuration ) )
            {
                return default;
            }

            // Run the pipeline.
            return await this.ExecuteCoreAsync(
                diagnosticAdder,
                partialCompilation,
                resources,
                configuration,
                cancellationToken );
        }

        public async Task<FallibleResult<CompileTimeAspectPipelineResult>> ExecuteCoreAsync(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            ImmutableArray<ManagedResource> resources,
            AspectPipelineConfiguration configuration,
            CancellationToken cancellationToken )
        {
            try
            {
                // Execute the pipeline.
                var result = await this.ExecuteAsync( compilation, diagnosticAdder, configuration, cancellationToken );

                if ( !result.IsSuccess )
                {
                    return default;
                }

                var resultPartialCompilation = result.Value.Compilation;

                // Execute validators.
                IReadOnlyList<ReferenceValidatorInstance> referenceValidators = result.Value.ExternallyVisibleValidators;

                // Format the output.
                if ( this.ProjectOptions.FormatOutput && OutputCodeFormatter.CanFormat )
                {
                    // ReSharper disable once AccessToModifiedClosure
                    resultPartialCompilation = await OutputCodeFormatter.FormatToSyntaxAsync( resultPartialCompilation, cancellationToken );
                }

                // Add managed resources.
                ImmutableArray<ManagedResource> additionalResources;

                if ( resultPartialCompilation.Resources.IsDefaultOrEmpty )
                {
                    additionalResources = ImmutableArray<ManagedResource>.Empty;
                }
                else
                {
                    additionalResources = resultPartialCompilation.Resources.Where( r => !resources.Contains( r ) ).ToImmutableArray();
                }

                if ( configuration.CompileTimeProject is { IsEmpty: false } )
                {
                    additionalResources = additionalResources.Add( configuration.CompileTimeProject.ToResource() );
                }

                // Create a manifest for transitive aspects and validators.
                if ( result.Value.ExternallyInheritableAspects.Length > 0 || referenceValidators.Count > 0 )
                {
                    var inheritedAspectsManifest = TransitiveAspectsManifest.Create(
                        result.Value.ExternallyInheritableAspects.Select( i => new InheritableAspectInstance( i ) ).ToImmutableArray(),
                        referenceValidators.Select( i => new TransitiveValidatorInstance( i ) ).ToImmutableArray() );

                    var resource = inheritedAspectsManifest.ToResource( configuration.ServiceProvider );
                    additionalResources = additionalResources.Add( resource );
                }

                var resultingCompilation = (PartialCompilation) await RunTimeAssemblyRewriter.RewriteAsync( resultPartialCompilation, this.ServiceProvider );
                var syntaxTreeTransformations = resultingCompilation.ToTransformations();

                return new CompileTimeAspectPipelineResult(
                    syntaxTreeTransformations,
                    additionalResources,
                    resultingCompilation,
                    result.Value.AdditionalCompilationOutputFiles );
            }
            catch ( DiagnosticException exception )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    diagnosticAdder.Report( diagnostic );
                }

                return default;
            }
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new CompileTimePipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );
    }
}