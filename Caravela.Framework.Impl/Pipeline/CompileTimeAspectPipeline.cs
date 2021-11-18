// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.Pipeline
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
            AspectExecutionScenario executionScenario = AspectExecutionScenario.CompileTime ) : base(
            serviceProvider,
            executionScenario,
            isTest,
            domain )
        {
            if ( this.ProjectOptions.DebugCompilerProcess )
            {
                if ( !Debugger.IsAttached )
                {
                    Debugger.Launch();
                }
            }
        }

        public async Task<CompileTimeAspectPipelineResult?> ExecuteAsync(
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            ImmutableArray<ManagedResource> resources,
            CancellationToken cancellationToken )
        {
            var partialCompilation = PartialCompilation.CreateComplete( compilation );

            // Skip if Caravela has been disabled for this project.
            if ( !this.ProjectOptions.IsFrameworkEnabled )
            {
                return new CompileTimeAspectPipelineResult(
                    ImmutableArray<SyntaxTreeTransformation>.Empty,
                    ImmutableArray<ManagedResource>.Empty,
                    partialCompilation );
            }

            
            // Run the code analyzers that normally run at design time.
            if ( !TemplatingCodeValidator.Validate( compilation, diagnosticAdder, this.ServiceProvider, cancellationToken ) )
            {
                return null;
            }

            // Initialize the pipeline and generate the compile-time project.
            if ( !this.TryInitialize( diagnosticAdder, partialCompilation, null, cancellationToken, out var configuration ) )
            {
                return null;
            }

            return await this.ExecuteCoreAsync(
                diagnosticAdder,
                partialCompilation,
                resources,
                configuration,
                cancellationToken );
        }

        internal async Task<CompileTimeAspectPipelineResult?> ExecuteCoreAsync(
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

                var resultPartialCompilation = result.PartialCompilation;

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

                // Add the index of inherited aspects.
                if ( result.ExternallyInheritableAspects.Length > 0 )
                {
                    var inheritedAspectsManifest = InheritableAspectsManifest.Create(
                        result.ExternallyInheritableAspects,
                        resultPartialCompilation.Compilation );

                    var resource = inheritedAspectsManifest.ToResource();
                    additionalResources = additionalResources.Add( resource );
                }

                var resultingCompilation = (PartialCompilation) RunTimeAssemblyRewriter.Rewrite( resultPartialCompilation, this.ServiceProvider );
                var syntaxTreeTransformations = resultingCompilation.ToTransformations();

                return new CompileTimeAspectPipelineResult( syntaxTreeTransformations, additionalResources, resultingCompilation );
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
            => new CompileTimePipelineStage( compileTimeProject, configuration.Parts, this.ServiceProvider );
    }
}