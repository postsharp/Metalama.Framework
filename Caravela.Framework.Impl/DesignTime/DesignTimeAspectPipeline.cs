// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// The main entry point of Caravela when called from a Roslyn source generator.
    /// </summary>
    internal class DesignTimeAspectPipeline : AspectPipeline
    {
        public DesignTimeAspectPipeline( IAspectPipelineContext context ) : base( context ) { }

        public bool TryExecute( [NotNullWhen( true )] out DesignTimeAspectPipelineResult result )
        {
            DiagnosticList diagnosticList = new();
            var success = this.TryExecuteCore( diagnosticList, out var pipelineResult );

            result = new DesignTimeAspectPipelineResult(
                pipelineResult?.AdditionalSyntaxTrees,
                new ImmutableDiagnosticList(
                    diagnosticList.ToImmutableArray(),
                    pipelineResult?.Diagnostics.DiagnosticSuppressions ) );

            return success;
        }

        public override bool CanTransformCompilation => false;

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new SourceGeneratorPipelineStage( parts, compileTimeAssemblyLoader, this );
    }
}