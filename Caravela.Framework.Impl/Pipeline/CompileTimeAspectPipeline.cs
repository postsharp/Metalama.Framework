// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used at compile time.
    /// </summary>
    public class CompileTimeAspectPipeline : AspectPipeline
    {
        public CompileTimeAspectPipeline( IBuildOptions buildOptions, CompileTimeDomain domain, IAssemblyLocator? assemblyLocator = null ) : base(
            buildOptions,
            domain,
            assemblyLocator )
        {
            if ( this.BuildOptions.DebugCompilerProcess )
            {
                if ( !Debugger.IsAttached )
                {
                    Debugger.Launch();
                }
            }
        }

        public bool TryExecute(
            IDiagnosticAdder diagnosticAdder,
            Compilation compilation,
            [NotNullWhen( true )] out Compilation? outputCompilation,
            [NotNullWhen( true )] out IReadOnlyList<ResourceDescription>? additionalResources )
        {
            try
            {
                var partialCompilation = PartialCompilation.CreateComplete( compilation );

                if ( !this.TryInitialize( diagnosticAdder, partialCompilation, null, out var configuration ) )
                {
                    outputCompilation = null;
                    additionalResources = null;

                    return false;
                }

                if ( !TryExecuteCore( partialCompilation, diagnosticAdder, configuration, out var result ) )
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

                if ( configuration.CompileTimeProject != null &&
                     result.PartialCompilation.Compilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary )
                {
                    additionalResourcesBuilder.Add( configuration.CompileTimeProject!.ToResource() );
                }

                outputCompilation = CompileTimeCompilationBuilder.PrepareRunTimeAssembly( result.PartialCompilation.Compilation );
                additionalResources = additionalResourcesBuilder;

                return true;
            }
            catch ( InvalidUserCodeException exception )
            {
                foreach ( var diagnostic in exception.Diagnostics )
                {
                    diagnosticAdder.Report( diagnostic );
                }

                outputCompilation = null;
                additionalResources = null;

                return false;
            }
        }

        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader )
            => new CompileTimePipelineStage( compileTimeProject, parts, this );

        public override bool CanTransformCompilation => true;
    }
}