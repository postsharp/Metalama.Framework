// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{

    /// <summary>
    /// The main entry point of Caravela when called from a Roslyn source generator.
    /// </summary>
    internal class SourceGeneratorAspectPipeline : AspectPipeline
    {
        public SourceGeneratorAspectPipeline( IAspectPipelineContext context ) : base( context )
        {
        }

        public bool TryExecute( [NotNullWhen( true )] out IImmutableDictionary<string, SyntaxTree>? additionalSyntaxTrees )
        {

            if ( !this.TryExecuteCore( out var result ) )
            {
                additionalSyntaxTrees = null;
                return false;
            }

            additionalSyntaxTrees = result.AssertNotNull().AdditionalSyntaxTrees;
            return true;
        }

        public override bool CanTransformCompilation => false;

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateStage( IReadOnlyList<OrderedAspectLayer> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new SourceGeneratorPipelineStage( parts, compileTimeAssemblyLoader, this );
    }
}
