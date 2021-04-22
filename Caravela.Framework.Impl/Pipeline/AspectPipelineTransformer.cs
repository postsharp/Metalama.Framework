﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [Transformer]
    internal sealed partial class AspectPipelineTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext transformerContext )
        {
            var context = new AspectPipelineContext( transformerContext );
            using CompileTimeAspectPipeline pipeline = new( context );

            if ( pipeline.TryExecute( context, out var compilation ) )
            {
                return compilation;
            }

            return transformerContext.Compilation;
        }
    }
}