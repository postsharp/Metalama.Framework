﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used at compile time.
    /// </summary>
    internal class CompileTimeAspectPipeline : AspectPipeline
    {
        private CompileTimeAspectPipeline( IAspectPipelineContext context ) : base( context, new Options() )
        {
        }

        public static bool TryExecute( IAspectPipelineContext context, [NotNullWhen( true )] out Compilation? outputCompilation )
        {
            try
            {

                var pipeline = new CompileTimeAspectPipeline( context );

                if ( !pipeline.TryExecute( out var result ) )
                {
                    outputCompilation = null;
                    return false;
                }

                foreach ( var resource in result.Resources )
                {
                    pipeline.Context.ManifestResources.Add( resource );
                }

                if ( result.Compilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary )
                {
                    var compileTimeAssembly = pipeline.CompileTimeAssemblyBuilder.EmitCompileTimeAssembly( result.Compilation );

                    if ( compileTimeAssembly != null )
                    {
                        pipeline.Context.ManifestResources.Add( new ResourceDescription(
                            pipeline.CompileTimeAssemblyBuilder.GetResourceName(), () => compileTimeAssembly, isPublic: true ) );
                    }
                }

                outputCompilation = pipeline.CompileTimeAssemblyBuilder.PrepareRunTimeAssembly( result.Compilation );
                return true;
            }
            catch ( Exception exception )
            {
                HandleException( exception, context );
                outputCompilation = null;
                return false;
            }
        }

        private class Options : IAspectPipelineOptions
        {
            public bool CanTransformCompilation => true;
        }

        protected override HighLevelAspectsPipelineStage CreateStage( IReadOnlyList<AspectPart> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new CompileTimeHighLevelAspectsPipelineStage( parts, compileTimeAssemblyLoader, this.PipelineOptions );
    }
}
