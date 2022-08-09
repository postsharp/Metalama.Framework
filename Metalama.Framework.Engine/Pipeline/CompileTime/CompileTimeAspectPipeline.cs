// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
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
            IExecutionScenario? executionScenario = null ) : base(
            serviceProvider,
            executionScenario ?? ExecutionScenario.CompileTime,
            isTest,
            domain ) { }

        public async Task<CompileTimeAspectPipelineResult?> ExecuteAsync(
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

            // Validate the code (some validations are not done by the template compiler).
            var isTemplatingCodeValidatorSuccessful = true;

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                var semanticModel = compilation.GetSemanticModel( syntaxTree );

                isTemplatingCodeValidatorSuccessful &= TemplatingCodeValidator.Validate(
                    this.ServiceProvider,
                    semanticModel,
                    diagnosticAdder.Report,
                    false,
                    false,
                    cancellationToken );
            }

            if ( !isTemplatingCodeValidatorSuccessful )
            {
                return null;
            }

            // TODO: initialize ProjectLicenseInfo from the license key specified in the project, but only if it is a redistribution one.
            // Get the value from the licensing service? 
            var projectLicenseInfo = ProjectLicenseInfo.Empty;

            // Initialize the pipeline and generate the compile-time project.
            if ( !this.TryInitialize( diagnosticAdder, partialCompilation, projectLicenseInfo, null, cancellationToken, out var configuration ) )
            {
                return null;
            }

            // Run the pipeline.
            return await this.ExecuteCoreAsync(
                diagnosticAdder,
                partialCompilation,
                resources,
                configuration,
                cancellationToken );
        }

        public async Task<CompileTimeAspectPipelineResult?> ExecuteCoreAsync(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            ImmutableArray<ManagedResource> resources,
            AspectPipelineConfiguration configuration,
            CancellationToken cancellationToken )
        {
            try
            {
                // Execute the pipeline.
                if ( !this.TryExecute( compilation, diagnosticAdder, configuration, cancellationToken, out var result ) )
                {
                    return null;
                }

                var resultPartialCompilation = result.Compilation;

                // Execute validators.
                IReadOnlyList<ReferenceValidatorInstance> referenceValidators = result.ExternallyVisibleValidators;

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
                if ( result.ExternallyInheritableAspects.Length > 0 || referenceValidators.Count > 0 )
                {
                    var inheritedAspectsManifest = TransitiveAspectsManifest.Create(
                        result.ExternallyInheritableAspects.Select( i => new InheritableAspectInstance( i ) ).ToImmutableArray(),
                        referenceValidators.Select( i => new TransitiveValidatorInstance( i ) ).ToImmutableArray() );

                    var resource = inheritedAspectsManifest.ToResource( configuration.ServiceProvider );
                    additionalResources = additionalResources.Add( resource );
                }

                var resultingCompilation = (PartialCompilation) RunTimeAssemblyRewriter.Rewrite( resultPartialCompilation, this.ServiceProvider );
                var syntaxTreeTransformations = resultingCompilation.ToTransformations();

                return new CompileTimeAspectPipelineResult(
                    syntaxTreeTransformations,
                    additionalResources,
                    resultingCompilation,
                    result.AdditionalCompilationOutputFiles );
            }
            catch ( DiagnosticException exception )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    diagnosticAdder.Report( diagnostic );
                }

                return null;
            }
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new CompileTimePipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );
    }
}