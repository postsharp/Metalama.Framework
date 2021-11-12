// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
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
            IPipelineStepsResult pipelineStepResult,
            CancellationToken cancellationToken )
        {
            var linker = new AspectLinker(
                pipelineConfiguration.ServiceProvider,
                new AspectLinkerInput(
                    input.PartialCompilation,
                    pipelineStepResult.Compilation,
                    pipelineStepResult.NonObservableTransformations,
                    input.AspectLayers,
                    input.Diagnostics.DiagnosticSuppressions.Concat( pipelineStepResult.Diagnostics.DiagnosticSuppressions ),
                    this._compileTimeProject ) );

            var linkerResult = linker.ToResult();

            var projectOptions = this.ServiceProvider.GetOptionalService<IProjectOptions>();
            IReadOnlyList<AuxiliaryFile>? fallbackFiles = null;

            if ( projectOptions != null && !projectOptions.DesignTimeEnabled )
            {
                fallbackFiles = this.GenerateDesignTimeFallbackFiles( pipelineConfiguration, input, projectOptions.AssertNotNull(), cancellationToken );
            }

            return new PipelineStageResult(
                linkerResult.Compilation,
                input.Project,
                input.AspectLayers,
                pipelineStepResult.Diagnostics.Concat( linkerResult.Diagnostics ),
                pipelineStepResult.ExternalAspectSources,
                input.ExternallyInheritableAspects.AddRange( pipelineStepResult.InheritableAspectInstances ),
                auxiliaryFiles: fallbackFiles != null ? input.AuxiliaryFiles.AddRange( fallbackFiles ) : input.AuxiliaryFiles );
        }

        private IReadOnlyList<AuxiliaryFile> GenerateDesignTimeFallbackFiles(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IProjectOptions buildOptions,
            CancellationToken cancellationToken )
        {
            var generatedFiles = new List<AuxiliaryFile>();
            var pipelineStage = new SourceGeneratorPipelineStage( this.CompileTimeProject, input.AspectLayers, this.ServiceProvider );

            // Ignore diagnostics, because these will be coming from the analyzer.
            if ( pipelineStage.TryExecute( pipelineConfiguration, input, NullDiagnosticAdder.Instance, cancellationToken, out var stageResult ) )
            {
                foreach ( var syntaxTree in stageResult.AdditionalSyntaxTrees )
                {
                    var path = Path.GetDirectoryName( syntaxTree.Name );
                    var name = Path.GetFileNameWithoutExtension( syntaxTree.Name );
                    var ext = Path.GetExtension( syntaxTree.Name );
                    var relativePath = Path.Combine( path, $"{name}.g{ext}" );
                    var content = (syntaxTree.GeneratedSyntaxTree.Encoding ?? Encoding.UTF8).GetBytes( syntaxTree.GeneratedSyntaxTree.ToString() );
                    generatedFiles.Add( new GeneratedAuxiliaryFile( relativePath, AuxiliaryFileKind.DesignTimeFallback, content ) );
                }
            }

            generatedFiles.Add(
                new GeneratedAuxiliaryFile( ".touch", AuxiliaryFileKind.DesignTimeFallback, Encoding.UTF8.GetBytes( Guid.NewGuid().ToString() ) ) );

            return generatedFiles;
        }
    }
}