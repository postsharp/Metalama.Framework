// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        private readonly IAspectPipelineContext _context;

        public CompileTimeAspectPipeline( IAspectPipelineContext context ) : base( context.BuildOptions )
        {
            this._context = context;
        }

        public bool TryExecute( IDiagnosticAdder diagnosticAdder,
                                 Compilation compilation, 
                                [NotNullWhen( true )] out Compilation? outputCompilation, 
                                [NotNullWhen( true )] out IReadOnlyList<ResourceDescription>? additionalResources )
        {
            try
            {
                var compilationModel = CompilationModel.CreateInitialInstance( compilation );

                if ( !this.Initialize( diagnosticAdder, compilationModel, this._context.Plugins, out var configuration ) )
                {
                    outputCompilation = null;
                    additionalResources = null;

                    return false;
                }
                
                if ( !this.TryExecuteCore( compilationModel, diagnosticAdder, configuration, out var result ) )
                {
                    outputCompilation = null;
                    additionalResources = null;

                    return false;
                }

                // Add managed resources.
                List<ResourceDescription> additionalResourcesBuilder = new();
                
                foreach ( var resource in result.Resources )
                {
                    additionalResourcesBuilder.Add( resource );
                }

                if ( result.Compilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary )
                {
                    additionalResourcesBuilder.Add(
                        new ResourceDescription( CompileTimeCompilationBuilder.ResourceName, () => configuration.CompileTimeProject.Serialize(), true ) );
                }

                outputCompilation = CompileTimeCompilationBuilder.PrepareRunTimeAssembly( result.Compilation );
                additionalResources = additionalResourcesBuilder;

                return true;
            }
            catch ( InvalidUserCodeException exception )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    diagnosticAdder.ReportDiagnostic( diagnostic );
                }

                outputCompilation = null;
                additionalResources = null;

                return false;
            }
            catch ( Exception exception ) when ( this._context.HandleExceptions )
            {
                this.HandleException( exception, diagnosticAdder );
                
                outputCompilation = null;
                additionalResources = null;

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