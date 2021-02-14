using Caravela.Framework.Impl.CompileTime;
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
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

                if ( !pipeline.TryExecute( out var result ))
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

            public bool CanAddSyntaxTrees => false;
        }

        protected override AdviceWeaverStage CreateAdviceWeaverStage( IReadOnlyList<AspectPart> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader ) 
            =>new CompileTimeAdviceWeaverStage( parts, compileTimeAssemblyLoader );
    }
}
