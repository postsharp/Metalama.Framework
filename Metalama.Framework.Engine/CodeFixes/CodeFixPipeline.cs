// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
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
            ProjectServiceProvider serviceProvider,
            CompileTimeDomain? domain,
            string diagnosticId,
            string diagnosticFilePath,
            in TextSpan diagnosticSpan ) :
            base( serviceProvider, ExecutionScenario.CodeFix, domain )
        {
            this._diagnosticId = diagnosticId;
            this._diagnosticFilePath = diagnosticFilePath;
            this._diagnosticSpan = diagnosticSpan;
        }

        private protected override bool FilterCodeFix( IDiagnosticDefinition diagnosticDefinition, Location location )
            => diagnosticDefinition.Id == this._diagnosticId &&
               location.SourceTree?.FilePath == this._diagnosticFilePath &&
               location.SourceSpan.Equals( this._diagnosticSpan );

        public async Task<FallibleResultWithDiagnostics<CodeFixPipelineResult>> ExecuteAsync(
            PartialCompilation partialCompilation,
            AspectPipelineConfiguration? configuration,
            TestableCancellationToken cancellationToken )
        {
            var diagnostics = new DiagnosticBag();

            if ( configuration == null )
            {
                if ( !this.TryInitialize( diagnostics, partialCompilation, null, null, cancellationToken, out configuration ) )
                {
                    return FallibleResultWithDiagnostics<CodeFixPipelineResult>.Failed( diagnostics.ToImmutableArray() );
                }
            }

            var pipelineResult = await this.ExecuteAsync( partialCompilation, diagnostics, configuration, cancellationToken );

            if ( !pipelineResult.IsSuccessful )
            {
                return FallibleResultWithDiagnostics<CodeFixPipelineResult>.Failed( diagnostics.ToImmutableArray() );
            }

            var codeFixes = pipelineResult.Value.Diagnostics.CodeFixes;
            var finalCompilation = pipelineResult.Value.CompilationModels[^1];

            // Run the validators.
            if ( !pipelineResult.Value.ValidatorSources.IsDefaultOrEmpty )
            {
                var validationRunner = new ValidationRunner( configuration, pipelineResult.Value.ValidatorSources, cancellationToken );
                var initialCompilation = pipelineResult.Value.CompilationModels[0];
                var validationResult = validationRunner.RunAll( initialCompilation, finalCompilation );

                codeFixes = codeFixes.AddRange( validationResult.Diagnostics.CodeFixes );
            }

            return FallibleResultWithDiagnostics<CodeFixPipelineResult>.Succeeded( new CodeFixPipelineResult( configuration, finalCompilation, codeFixes ) );
        }
    }

    internal record CodeFixPipelineResult( AspectPipelineConfiguration Configuration, CompilationModel Compilation, ImmutableArray<CodeFixInstance> CodeFixes );
}