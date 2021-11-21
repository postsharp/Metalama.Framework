// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    internal class CodeFixPipeline : AspectPipeline
    {
        private readonly Diagnostic _diagnostic;

        public CodeFixPipeline(
            ServiceProvider serviceProvider,
            bool isTest,
            CompileTimeDomain? domain,
            Diagnostic diagnostic ) :
            base( serviceProvider, ExecutionScenario.CodeFix, isTest, domain )
        {
            this._diagnostic = diagnostic;
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new CodeFixPipelineStage( compileTimeProject, configuration.Parts, this.ServiceProvider );

        private protected override LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration, CompileTimeProject compileTimeProject )
            => null;

        private protected override bool FilterCodeFix( IDiagnosticDefinition diagnosticDefinition, Location location )
            => diagnosticDefinition.Id == this._diagnostic.Id &&
               location.SourceTree == this._diagnostic.Location.SourceTree &&
               location.SourceSpan.Equals( this._diagnostic.Location.SourceSpan );

        public bool TryExecute(
            PartialCompilation partialCompilation,
            AspectPipelineConfiguration configuration,
            CancellationToken cancellationToken,
            out ImmutableArray<CodeFixInstance> codeFixes,
            [NotNullWhen( true )] out CompilationModel? compilationModel )
        {
            if ( !this.TryExecute( partialCompilation, NullDiagnosticAdder.Instance, configuration, cancellationToken, out var result ) )
            {
                codeFixes = default;
                compilationModel = null;

                return false;
            }
            else
            {
                codeFixes = result.Diagnostics.CodeFixes;
                compilationModel = result.CompilationModel.AssertNotNull();

                return true;
            }
        }
    }
}