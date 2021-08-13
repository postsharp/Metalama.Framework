// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="AspectPipeline"/> used at compile time.
    /// </summary>
    public class CompileTimeAspectPipeline : AspectPipeline
    {
        public CompileTimeAspectPipeline(
            IProjectOptions projectOptions,
            bool isTest,
            CompileTimeDomain? domain = null,
            IPathOptions? directoryOptions = null,
            IAssemblyLocator? assemblyLocator = null ) : base(
            projectOptions,
            AspectExecutionScenario.CompileTime,
            isTest,
            domain,
            directoryOptions,
            assemblyLocator )
        {
            if ( this.ProjectOptions.DebugCompilerProcess )
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
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out Compilation? outputCompilation,
            [NotNullWhen( true )] out IReadOnlyList<ResourceDescription>? additionalResources )
        {
            if ( !this.ProjectOptions.IsFrameworkEnabled )
            {
                outputCompilation = compilation;
                additionalResources = Array.Empty<ResourceDescription>();

                return true;
            }

            try
            {
                if ( !TemplatingCodeValidator.Validate( compilation, diagnosticAdder, this.ServiceProvider, cancellationToken ) )
                {
                    outputCompilation = null;
                    additionalResources = null;

                    return false;
                }

                var partialCompilation = PartialCompilation.CreateComplete( compilation );

                // Initialize the pipeline and generate the compile-time project.
                if ( !this.TryInitialize( diagnosticAdder, partialCompilation, null, cancellationToken, out var configuration ) )
                {
                    outputCompilation = null;
                    additionalResources = null;

                    return false;
                }

                List<ResourceDescription> additionalResourcesBuilder = new();

                // Execute the pipeline.
                if ( !this.TryExecute( partialCompilation, diagnosticAdder, configuration, cancellationToken, out var result ) )
                {
                    outputCompilation = null;
                    additionalResources = null;

                    return false;
                }

                var resultCompilation = result.PartialCompilation;

                // Format the output.
                if ( this.ProjectOptions.FormatOutput && OutputCodeFormatter.CanFormat )
                {
                    // ReSharper disable once AccessToModifiedClosure
                    resultCompilation = Task.Run( () => OutputCodeFormatter.FormatToSyntaxAsync( resultCompilation, cancellationToken ), cancellationToken )
                        .Result;
                }

                // Add managed resources.
                foreach ( var resource in result.Resources )
                {
                    additionalResourcesBuilder.Add( resource );
                }

                if ( configuration.CompileTimeProject is { IsEmpty: false } )
                {
                    additionalResourcesBuilder.Add( configuration.CompileTimeProject!.ToResource() );
                }

                outputCompilation = RunTimeAssemblyRewriter.Rewrite( resultCompilation.Compilation, this.ServiceProvider );
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
            => new CompileTimePipelineStage( compileTimeProject, parts, this.ServiceProvider );
    }
}