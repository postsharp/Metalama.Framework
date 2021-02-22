using Caravela.Framework.Impl.AspectOrdering;
using System;
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
        public CompileTimeAspectPipeline( IAspectPipelineContext context ) : base( context )
        {
        }

        public bool TryExecute( [NotNullWhen( true )] out Compilation? outputCompilation )
        {
            try
            {

                var pipeline = new CompileTimeAspectPipeline( this.Context );

                if ( !pipeline.TryExecuteCore( out var result ) )
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
            catch ( Exception exception ) when ( this.Context.HandleExceptions )
            {
                this.HandleException( exception );
                outputCompilation = null;
                return false;
            }
        }

  

        protected override HighLevelPipelineStage CreateStage( IReadOnlyList<OrderedAspectLayer> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new CompileTimePipelineStage( parts, compileTimeAssemblyLoader, this );

        public override bool CanTransformCompilation => true;
    }
}
