// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class SourceTransformer : ISourceTransformer
    {
        public void Execute( TransformerContext context )
        {
            var serviceProvider = ServiceProviderFactory.GetServiceProvider();

            // Try.Caravela ships its own project options using the async-local service provider.
            var projectOptions = serviceProvider.GetOptionalService<IProjectOptions>();

            if ( projectOptions == null )
            {
                projectOptions = new ProjectOptions( context.GlobalOptions, context.Plugins );
                serviceProvider = serviceProvider.WithService( projectOptions );
            }

            serviceProvider = serviceProvider.WithProjectScopedServices( context.Compilation.References );

            try
            {
                using CompileTimeAspectPipeline pipeline = new( serviceProvider, false );

                // ReSharper disable once AccessToDisposedClosure
                var pipelineResult =
                    Task.Run(
                            () => pipeline.ExecuteAsync(
                                new DiagnosticAdderAdapter( context.ReportDiagnostic ),
                                context.Compilation,
                                context.Resources.ToImmutableArray(),
                                CancellationToken.None ) )
                        .Result;

                if ( pipelineResult != null )
                {
                    context.AddResources( pipelineResult.AdditionalResources );
                    context.AddSyntaxTreeTransformations( pipelineResult.SyntaxTreeTransformations );
                }
            }
            catch ( Exception e )
            {
                var mustRethrow = true;

                ServiceProviderFactory.AsyncLocalProvider.GetOptionalService<ICompileTimeExceptionHandler>()
                    ?.ReportException( e, context.ReportDiagnostic, out mustRethrow );

                if ( mustRethrow )
                {
                    throw;
                }
            }
        }
    }
}