// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.CompileTime
{
    /// <summary>
    /// The implementation of <see cref="HighLevelPipelineStage"/> used at compile time (not at design time).
    /// </summary>
    internal sealed class LinkerPipelineStage : HighLevelPipelineStage
    {
        private readonly CompileTimeProject _compileTimeProject;

        public LinkerPipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers )
            : base( compileTimeProject, aspectLayers )
        {
            this._compileTimeProject = compileTimeProject;
        }

        /// <inheritdoc/>
        protected override async Task<AspectPipelineResult> GetStageResultAsync(
            AspectPipelineConfiguration pipelineConfiguration,
            AspectPipelineResult input,
            IPipelineStepsResult pipelineStepsResult,
            TestableCancellationToken cancellationToken )
        {
            // TODO: validators should not run here but after all pipeline stages. If there are several high-level stages, they may run several times.
            // Run the validators.
            var validationRunner = new ValidationRunner( pipelineConfiguration, pipelineStepsResult.ValidatorSources );
            var initialCompilation = pipelineStepsResult.FirstCompilation;
            var finalCompilation = pipelineStepsResult.LastCompilation;

            var validationResult = await validationRunner.RunAllAsync( initialCompilation, finalCompilation, cancellationToken );

            // Run the linker.
            var linker = new AspectLinker(
                pipelineConfiguration.ServiceProvider,
                new AspectLinkerInput(
                    pipelineStepsResult.LastCompilation,
                    input.FirstCompilationModel.AssertNotNull(),
                    pipelineStepsResult.Transformations,
                    input.AspectLayers,
                    this._compileTimeProject ) );

            var linkerResult = await linker.ExecuteAsync( cancellationToken );

            // Generate additional output files.
            var projectOptions = pipelineConfiguration.ServiceProvider.GetService<IProjectOptions>();
            IReadOnlyList<AdditionalCompilationOutputFile>? additionalCompilationOutputFiles = null;

            if ( projectOptions is { IsDesignTimeEnabled: false } )
            {
                additionalCompilationOutputFiles = await GenerateAdditionalCompilationOutputFilesAsync(
                    pipelineConfiguration.ServiceProvider,
                    input,
                    pipelineStepsResult,
                    cancellationToken );
            }

            // Return the result.
            return
                new AspectPipelineResult(
                    linkerResult.Compilation,
                    input.Project,
                    input.AspectLayers,
                    input.FirstCompilationModel.AssertNotNull(),
                    null,
                    input.Configuration,
                    input.Diagnostics.Concat( pipelineStepsResult.Diagnostics ).Concat( linkerResult.Diagnostics ).Concat( validationResult.Diagnostics ),
                    new PipelineContributorSources(
                        input.ContributorSources.AspectSources.AddRange( pipelineStepsResult.OverflowAspectSources ),
                        input.ContributorSources.ValidatorSources.AddRange( pipelineStepsResult.ValidatorSources ),
                        input.ContributorSources.OptionsSources ),
                    input.ExternallyInheritableAspects.AddRange(
                        pipelineStepsResult.InheritableAspectInstances.Select( i => new InheritableAspectInstance( i ) ) ),
                    finalCompilation.Annotations,
                    validationResult.HasDeclarationValidator,
                    validationResult.ExternallyVisibleValidations,
                    additionalCompilationOutputFiles: additionalCompilationOutputFiles != null
                        ? input.AdditionalCompilationOutputFiles.AddRange( additionalCompilationOutputFiles )
                        : input.AdditionalCompilationOutputFiles,
                    aspectInstanceResults: input.AspectInstanceResults.AddRange( pipelineStepsResult.AspectInstanceResults ) );
        }

        private static async Task<IReadOnlyList<AdditionalCompilationOutputFile>> GenerateAdditionalCompilationOutputFilesAsync(
            ProjectServiceProvider serviceProvider,
            AspectPipelineResult input,
            IPipelineStepsResult pipelineStepResult,
            TestableCancellationToken cancellationToken )
        {
            var generatedFiles = new List<AdditionalCompilationOutputFile>();

            // TODO: We don't need these diagnostics, but we cannot pass NullDiagnosticAdder here.
            var diagnostics = new UserDiagnosticSink();

            var additionalSyntaxTrees = await DesignTimeSyntaxTreeGenerator.GenerateDesignTimeSyntaxTreesAsync(
                serviceProvider,
                input.LastCompilation,
                pipelineStepResult.FirstCompilation,
                pipelineStepResult.LastCompilation,
                pipelineStepResult.Transformations,
                diagnostics,
                cancellationToken );

            // Ignore diagnostics, because these will be coming from the analyzer.
            var uniquePaths = new HashSet<string>();

            foreach ( var syntaxTree in additionalSyntaxTrees )
            {
                var path = Path.GetDirectoryName( syntaxTree.Name )!;
                var name = Path.GetFileNameWithoutExtension( syntaxTree.Name );
                var ext = Path.GetExtension( syntaxTree.Name );
                var relativePath = Path.Combine( path, $"{name}.g{ext}" );
                relativePath = GetUniqueFilename( relativePath );

                generatedFiles.Add(
                    new GeneratedAdditionalCompilationOutputFile(
                        relativePath,
                        AdditionalCompilationOutputFileKind.DesignTimeGeneratedCode,
                        stream =>
                        {
                            using var writer = new StreamWriter( stream, syntaxTree.GeneratedSyntaxTree.Encoding ?? Encoding.UTF8 );
                            writer.Write( syntaxTree.GeneratedSyntaxTree.ToString() );
                        } ) );
            }

            generatedFiles.Add(
                new GeneratedAdditionalCompilationOutputFile(
                    "touch",
                    AdditionalCompilationOutputFileKind.DesignTimeTouch,
                    stream =>
                    {
                        using var writer = new StreamWriter( stream, Encoding.UTF8 );
                        writer.Write( Guid.NewGuid() );
                    } ) );

            return generatedFiles;

            string GetUniqueFilename( string filename )
            {
                if ( !uniquePaths.Add( filename ) )
                {
                    for ( var i = 1; /* Intentionally empty */; i++ )
                    {
                        var path = Path.GetDirectoryName( filename )!;
                        var name = Path.GetFileNameWithoutExtension( filename );
                        var ext = Path.GetExtension( filename );
                        var relativePath = Path.Combine( path, $"{name}.g.{i}{ext}" );

                        if ( uniquePaths.Add( relativePath ) )
                        {
                            return relativePath;
                        }
                    }
                }

                return filename;
            }
        }
    }
}