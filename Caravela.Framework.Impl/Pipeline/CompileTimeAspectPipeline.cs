// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used at compile time.
    /// </summary>
    public class CompileTimeAspectPipeline : AspectPipeline
    {
        public CompileTimeAspectPipeline( IAspectPipelineContext context ) : base( context )
        {
        }

        public bool TryExecute( [NotNullWhen( true )] out Compilation? outputCompilation )
        {
            try
            {

                if ( !this.TryExecuteCore( out var result ) )
                {
                    outputCompilation = null;
                    return false;
                }

                foreach ( var resource in result.Resources )
                {
                    this.Context.ManifestResources.Add( resource );
                }

                var compileTimeAssemblyBuilder = this.CompileTimeAssemblyBuilder.AssertNotNull();

                if ( result.Compilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary )
                {
                    var compileTimeAssembly = compileTimeAssemblyBuilder.BuiltAssemblies[this.Context.Compilation.AssemblyName!];

                    if ( compileTimeAssembly != null )
                    {
                        this.Context.ManifestResources.Add( new ResourceDescription(
                            compileTimeAssemblyBuilder.GetResourceName(), () => compileTimeAssembly, isPublic: true ) );
                    }
                }

                outputCompilation = compileTimeAssemblyBuilder.PrepareRunTimeAssembly( result.Compilation );
                return true;
            }
            catch ( InvalidUserCodeException exception )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    this.Context.ReportDiagnostic( diagnostic );
                }

                outputCompilation = null;
                return false;
            }
            catch ( Exception exception ) when ( this.Context.HandleExceptions )
            {
                this.HandleException( exception );
                outputCompilation = null;
                return false;
            }
        }

        private protected override HighLevelPipelineStage CreateStage( IReadOnlyList<OrderedAspectLayer> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new CompileTimePipelineStage( parts, compileTimeAssemblyLoader, this );

        public override bool CanTransformCompilation => true;
    }
}
