// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

        public bool TryExecute(
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            ImmutableArray<ManagedResource> resources,
            CancellationToken cancellationToken,
            out ImmutableArray<SyntaxTreeTransformation> syntaxTreeTransformations,
            out ImmutableArray<ManagedResource> additionalResources,
            [NotNullWhen( true )] out Compilation? resultingCompilation )
        {
            // Run the code analyzers that normally run at design time.
            if ( !TemplatingCodeValidator.Validate( compilation, diagnosticAdder, this.ServiceProvider, cancellationToken ) )
            {
                resultingCompilation = null;

                return false;
            }

            var partialCompilation = PartialCompilation.CreateComplete( compilation );

            // Initialize the pipeline and generate the compile-time project.
            if ( !this.TryInitialize( diagnosticAdder, partialCompilation, null, cancellationToken, out var configuration ) )
            {
                resultingCompilation = null;

                return false;
            }

            if ( this.TryExecuteCore(
                diagnosticAdder,
                partialCompilation,
                resources,
                configuration,
                cancellationToken,
                out syntaxTreeTransformations,
                out additionalResources,
                out var resultingPartialCompilation ) )
            {
                resultingCompilation = resultingPartialCompilation.Compilation;

                return true;
            }
            else
            {
                resultingCompilation = null;

                return false;
            }
        }

        internal bool TryExecuteCore(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            ImmutableArray<ManagedResource> resources,
            AspectProjectConfiguration configuration,
            CancellationToken cancellationToken,
            out ImmutableArray<SyntaxTreeTransformation> syntaxTreeTransformations,
            out ImmutableArray<ManagedResource> additionalResources,
            [NotNullWhen( true )] out PartialCompilation? resultingCompilation )
        {
            if ( !this.ProjectOptions.IsFrameworkEnabled )
            {
                syntaxTreeTransformations = ImmutableArray<SyntaxTreeTransformation>.Empty;
                additionalResources = ImmutableArray<ManagedResource>.Empty;
                resultingCompilation = compilation;

                return true;
            }

            syntaxTreeTransformations = default;
            additionalResources = default;
            resultingCompilation = null;

            try
            {
                // Execute the pipeline.
                if ( !this.TryExecute( compilation, diagnosticAdder, configuration, cancellationToken, out var result ) )
                {
                    return false;
                }

                var resultPartialCompilation = result.PartialCompilation;

                // Format the output.
                if ( this.ProjectOptions.FormatOutput && OutputCodeFormatter.CanFormat )
                {
                    // ReSharper disable once AccessToModifiedClosure
                    resultPartialCompilation = Task.Run(
                            () => OutputCodeFormatter.FormatToSyntaxAsync( resultPartialCompilation, cancellationToken ),
                            cancellationToken )
                        .Result;
                }

                // Add managed resources.
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
                    var inheritedAspectsManifest = InheritableAspectsManifest.Create( result.ExternallyInheritableAspects );
                    var resource = inheritedAspectsManifest.ToResource();
                    additionalResources = additionalResources.Add( resource );
                }

                resultingCompilation = (PartialCompilation) RunTimeAssemblyRewriter.Rewrite( resultPartialCompilation, this.ServiceProvider );
                syntaxTreeTransformations = resultPartialCompilation.ToTransformations();

                return true;
            }
            catch ( InvalidUserCodeException exception )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    diagnosticAdder.Report( diagnostic );
                }

                syntaxTreeTransformations = default;

                return false;
            }
        }

        private protected override HighLevelPipelineStage CreateStage(
            ImmutableArray<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject )
            => new CompileTimePipelineStage( compileTimeProject, parts, this.ServiceProvider );

        internal static PipelineStage MapStage( AspectProjectConfiguration configuration, PipelineStage stage )
            => stage switch
            {
                SourceGeneratorPipelineStage => new CompileTimePipelineStage(
                    configuration.CompileTimeProject!,
                    configuration.AspectLayers,
                    stage.ServiceProvider ),
                _ => stage
            };
    }
}