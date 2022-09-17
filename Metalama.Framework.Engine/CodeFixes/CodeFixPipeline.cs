// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
using System.Threading.Tasks;

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

        private protected override LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration ) => null;

        private protected override bool FilterCodeFix( IDiagnosticDefinition diagnosticDefinition, Location location )
            => diagnosticDefinition.Id == this._diagnosticId &&
               location.SourceTree?.FilePath == this._diagnosticFilePath &&
               location.SourceSpan.Equals( this._diagnosticSpan );

        public async Task<FallibleResult<CodeFixPipelineResult>> ExecuteAsync(
            PartialCompilation partialCompilation,
            AspectPipelineConfiguration? configuration,
            CancellationToken cancellationToken )
        {
            if ( configuration == null )
            {
                if ( !this.TryInitialize( NullDiagnosticAdder.Instance, partialCompilation, null, null, cancellationToken, out configuration ) )
                {
                    return default;
                }
            }

            var result = await this.ExecuteAsync( partialCompilation, NullDiagnosticAdder.Instance, configuration, cancellationToken );

            if ( !result.IsSuccess )
            {
                return default;
            }
            else
            {
                var codeFixes = result.Value.Diagnostics.CodeFixes;
                var compilation = result.Value.CompilationModels[result.Value.CompilationModels.Length - 1];

                return FallibleResult<CodeFixPipelineResult>.Succeeded( new CodeFixPipelineResult( configuration, compilation, codeFixes ) );
            }
        }
    }

    internal record CodeFixPipelineResult( AspectPipelineConfiguration Configuration, CompilationModel Compilation, ImmutableArray<CodeFixInstance> CodeFixes );
}