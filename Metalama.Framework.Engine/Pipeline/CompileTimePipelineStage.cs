// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.AdditionalOutputs;
using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.Collections;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.DesignTime.Pipeline;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Linking;
using Metalama.Framework.Impl.Options;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Metalama.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="HighLevelPipelineStage"/> used at compile time (not at design time).
    /// </summary>
    internal class CompileTimePipelineStage : HighLevelPipelineStage
    {
        private readonly CompileTimeProject _compileTimeProject;

        public CompileTimePipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            IServiceProvider serviceProvider )
            : base( compileTimeProject, aspectLayers, serviceProvider )
        {
            this._compileTimeProject = compileTimeProject;
        }

        /// <inheritdoc/>
        protected override PipelineStageResult GetStageResult(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepsResult,
            CancellationToken cancellationToken )
        {
            var linker = new AspectLinker(
                pipelineConfiguration.ServiceProvider,
                new AspectLinkerInput(
                    input.Compilation,
                    pipelineStepsResult.Compilation,
                    pipelineStepsResult.NonObservableTransformations,
                    input.AspectLayers,
                    input.Diagnostics.DiagnosticSuppressions.Concat( pipelineStepsResult.Diagnostics.DiagnosticSuppressions ),
                    this._compileTimeProject ) );

            var linkerResult = linker.ToResult();

            var projectOptions = this.ServiceProvider.GetOptionalService<IProjectOptions>();
            IReadOnlyList<AdditionalCompilationOutputFile>? additionalCompilationOutputFiles = null;

            if ( projectOptions != null && !projectOptions.IsDesignTimeEnabled )
            {
                additionalCompilationOutputFiles = this.GenerateAdditionalCompilationOutputFiles(
                    input,
                    pipelineStepsResult,
                    cancellationToken );
            }

            return new PipelineStageResult(
                linkerResult.Compilation,
                input.Project,
                input.AspectLayers,
                null,
                pipelineStepsResult.Diagnostics.Concat( linkerResult.Diagnostics ),
                pipelineStepsResult.ExternalAspectSources,
                input.ExternallyInheritableAspects.AddRange( pipelineStepsResult.InheritableAspectInstances ),
                additionalCompilationOutputFiles: additionalCompilationOutputFiles != null
                    ? input.AdditionalCompilationOutputFiles.AddRange( additionalCompilationOutputFiles )
                    : input.AdditionalCompilationOutputFiles );
        }

        private IReadOnlyList<AdditionalCompilationOutputFile> GenerateAdditionalCompilationOutputFiles(
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepResult,
            CancellationToken cancellationToken )
        {
            var generatedFiles = new List<AdditionalCompilationOutputFile>();

            // TODO: We don't need these diagnostics, but we cannot pass NullDiagnosticAdder here.
            var diagnostics = new UserDiagnosticSink();

            DesignTimeSyntaxTreeGenerator.GenerateDesignTimeSyntaxTrees(
                input.Compilation,
                pipelineStepResult.Compilation,
                this.ServiceProvider,
                diagnostics,
                cancellationToken,
                out var additionalSyntaxTrees );

            // Ignore diagnostics, because these will be coming from the analyzer.
            var uniquePaths = new HashSet<string>();

            foreach ( var syntaxTree in additionalSyntaxTrees )
            {
                var path = Path.GetDirectoryName( syntaxTree.Name );
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
                        var path = Path.GetDirectoryName( filename );
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