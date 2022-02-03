// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used to gather the code fix implementations, when a code fix
    /// is selected for preview or execution.
    /// </summary>
    internal class CodeFixPipeline : AspectPipeline
    {
        private readonly string _diagnosticId;
        private readonly string _diagnosticFilePath;
        private readonly TextSpan _diagnosticSpan;

        public CodeFixPipeline(
            ServiceProvider serviceProvider,
            bool isTest,
            CompileTimeDomain? domain,
            string diagnosticId,
            string diagnosticFilePath,
            in TextSpan diagnosticSpan ) :
            base( serviceProvider, ExecutionScenario.CodeFix, isTest, domain )
        {
            this._diagnosticId = diagnosticId;
            this._diagnosticFilePath = diagnosticFilePath;
            this._diagnosticSpan = diagnosticSpan;
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new CodeFixPipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );

        private protected override LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration, CompileTimeProject compileTimeProject )
            => null;

        private protected override bool FilterCodeFix( IDiagnosticDefinition diagnosticDefinition, Location location )
            => diagnosticDefinition.Id == this._diagnosticId &&
               location.SourceTree?.FilePath == this._diagnosticFilePath &&
               location.SourceSpan.Equals( this._diagnosticSpan );

        public bool TryExecute(
            PartialCompilation partialCompilation,
            ref AspectPipelineConfiguration? configuration,
            CancellationToken cancellationToken,
            out ImmutableArray<CodeFixInstance> codeFixes,
            [NotNullWhen( true )] out CompilationModel? compilationModel )
        {
            if ( configuration == null )
            {
                if ( !this.TryInitialize( NullDiagnosticAdder.Instance, partialCompilation, null, cancellationToken, out configuration ) )
                {
                    compilationModel = null;

                    return false;
                }
            }

            if ( !this.TryExecute( partialCompilation, NullDiagnosticAdder.Instance, configuration, cancellationToken, out var result ) )
            {
                codeFixes = default;
                compilationModel = null;

                return false;
            }
            else
            {
                codeFixes = result.Diagnostics.CodeFixes;
                compilationModel = result.CompilationModels[result.CompilationModels.Length - 1];

                return true;
            }
        }
    }
}