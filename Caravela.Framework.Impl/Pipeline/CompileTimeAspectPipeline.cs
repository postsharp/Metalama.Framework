// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used at compile time.
    /// </summary>
    public class CompileTimeAspectPipeline : AspectPipeline
    {
        public CompileTimeAspectPipeline( IAspectPipelineContext context ) : base( context ) { }

        public bool TryExecute( IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out Compilation? outputCompilation )
        {
            try
            {
                if ( !this.TryExecuteCore( diagnosticAdder, out var result, out var compileTimeProject ) )
                {
                    outputCompilation = null;

                    return false;
                }

                foreach ( var resource in result.Resources )
                {
                    this.Context.ManifestResources.Add( resource );
                }

                if ( result.Compilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary )
                {
                    this.Context.ManifestResources.Add(
                        new ResourceDescription( CompileTimeCompilationBuilder.ResourceName, () => compileTimeProject.Serialize(), true ) );
                }

                outputCompilation = CompileTimeCompilationBuilder.PrepareRunTimeAssembly( result.Compilation );

                return true;
            }
            catch ( InvalidUserCodeException exception )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    diagnosticAdder.ReportDiagnostic( diagnostic );
                }

                outputCompilation = null;

                return false;
            }
            catch ( Exception exception ) when ( this.Context.HandleExceptions )
            {
                this.HandleException( exception, diagnosticAdder );
                outputCompilation = null;

                return false;
            }
        }

        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new CompileTimePipelineStage( parts, this );

        public override bool CanTransformCompilation => true;
    }
}