// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.CompileTime
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used at compile time.
    /// </summary>
    public sealed class CompileTimeAspectPipeline : AspectPipeline
    {
        public CompileTimeAspectPipeline(
            ProjectServiceProvider serviceProvider,
            CompileTimeDomain? domain = null,
            ExecutionScenario? executionScenario = null ) : base(
            serviceProvider,
            executionScenario ?? ExecutionScenario.CompileTime,
            domain ) { }

        private bool VerifyLanguageVersion( Compilation compilation, IDiagnosticAdder diagnosticAdder )
        {
            // Note that Roslyn does not properly set the language version at design time, so we don't check the language version
            // in other pipelines.

            var languageVersion =
                (((CSharpParseOptions?) compilation.SyntaxTrees.FirstOrDefault()?.Options)?.LanguageVersion ?? LanguageVersion.Latest)
                .MapSpecifiedToEffectiveVersion();

            static string[] FormatSupportedVersions() => SupportedCSharpVersions.All.SelectAsArray( x => x.ToDisplayString() );

            if ( languageVersion == LanguageVersion.Preview )
            {
                if ( !this.ProjectOptions.AllowPreviewLanguageFeatures )
                {
                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.PreviewCSharpVersionNotSupported.CreateRoslynDiagnostic( null, FormatSupportedVersions() ) );

                    return false;
                }
            }
            else if ( !SupportedCSharpVersions.All.Contains( languageVersion ) )
            {
                diagnosticAdder.Report(
                    GeneralDiagnosticDescriptors.CSharpVersionNotSupported.CreateRoslynDiagnostic(
                        null,
                        (languageVersion.ToDisplayString(), FormatSupportedVersions()) ) );

                return false;
            }

            return true;
        }

        public async Task<FallibleResult<CompileTimeAspectPipelineResult>> ExecuteAsync(
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            ImmutableArray<ManagedResource> resources,
            TestableCancellationToken cancellationToken = default )
        {
            var compilationContext = this.ServiceProvider.GetRequiredService<ClassifyingCompilationContextFactory>().GetInstance( compilation );
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
                diagnosticAdder.Report( GeneralDiagnosticDescriptors.MissingMetalamaPreprocessorSymbol.CreateRoslynDiagnostic( null, null ) );

                return default;
            }

            // Validate the code (some validations are not done by the template compiler).
            var isTemplatingCodeValidatorSuccessful = await TemplatingCodeValidator.ValidateAsync(
                this.ServiceProvider,
                compilationContext,
                diagnosticAdder,
                cancellationToken );

            if ( !isTemplatingCodeValidatorSuccessful )
            {
                return default;
            }

            var licenseConsumptionService = this.ServiceProvider.GetService<IProjectLicenseConsumptionService>();

            var projectLicenseInfo = ProjectLicenseInfo.Get( licenseConsumptionService );

            if ( !this.VerifyLanguageVersion( compilation, diagnosticAdder ) )
            {
                return default;
            }

            // Initialize the pipeline and generate the compile-time project.
            if ( !this.TryInitialize( diagnosticAdder, partialCompilation.Compilation, projectLicenseInfo, null, cancellationToken, out var configuration ) )
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

        private async Task<FallibleResult<CompileTimeAspectPipelineResult>> ExecuteCoreAsync(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            ImmutableArray<ManagedResource> resources,
            AspectPipelineConfiguration configuration,
            TestableCancellationToken cancellationToken )
        {
            try
            {
                // Execute the pipeline.
                var result = await this.ExecuteAsync( compilation, diagnosticAdder, configuration, cancellationToken );

                if ( !result.IsSuccessful )
                {
                    return default;
                }

                var resultPartialCompilation = result.Value.LastCompilation;

                // Execute validators.
                IReadOnlyList<ReferenceValidatorInstance> referenceValidators = result.Value.ReferenceValidators;

                // Format the output.
                if ( this.ProjectOptions.FormatOutput || this.ProjectOptions.WriteHtml )
                {
                    // ReSharper disable once AccessToModifiedClosure
                    resultPartialCompilation = await OutputCodeFormatter.FormatAsync( resultPartialCompilation, cancellationToken );
                }

                // Write HTML (used only when building projects for documentation).
                if ( this.ProjectOptions.WriteHtml )
                {
                    await HtmlCodeWriter.WriteAllDiffAsync( this.ProjectOptions, this.ServiceProvider, compilation, resultPartialCompilation );
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
                var inheritableOptions =
                    result.Value.FirstCompilationModel.AssertNotNull().HierarchicalOptionsManager.GetInheritableOptions( result.Value.LastCompilationModel );

                var annotations = result.Value.LastCompilationModel?.GetExportedAnnotations()
                                  ?? ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation>.Empty;

                if ( result.Value.ExternallyInheritableAspects.Length > 0 || referenceValidators.Count > 0 || inheritableOptions.Count > 0
                     || !annotations.IsEmpty )
                {
                    var inheritedAspectsManifest = TransitiveAspectsManifest.Create(
                        result.Value.ExternallyInheritableAspects.Select( i => new InheritableAspectInstance( i ) )
                            .ToImmutableArray(),
                        referenceValidators.SelectAsImmutableArray( i => new TransitiveValidatorInstance( i ) ),
                        inheritableOptions,
                        annotations );

                    var resource = inheritedAspectsManifest.ToResource( configuration.ServiceProvider, compilation.Compilation );
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
            catch ( DiagnosticException exception ) when ( exception.InSourceCode )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    diagnosticAdder.Report( diagnostic );
                }

                return default;
            }
        }

        private protected override LowLevelPipelineStage CreateLowLevelStage( PipelineStageConfiguration configuration )
        {
            var partData = configuration.AspectLayers.Single();

            return new LowLevelPipelineStage( configuration.Weaver!, partData.AspectClass );
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new LinkerPipelineStage( compileTimeProject, configuration.AspectLayers );
    }
}