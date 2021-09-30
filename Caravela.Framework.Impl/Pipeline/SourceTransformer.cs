// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class SourceTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext context )
        {
            var projectOptions = new ProjectOptions( context.GlobalOptions, context.Plugins );

            var serviceProvider = ServiceProviderFactory.GetServiceProvider( assemblyLocator: new CompilationAssemblyLocator( context.Compilation ) )
                .WithService( projectOptions );

            try
            {
                using CompileTimeAspectPipeline pipeline = new( serviceProvider, false );

                if ( pipeline.TryExecute(
                    new DiagnosticAdderAdapter( context.ReportDiagnostic ),
                    context.Compilation,
                    context.ManifestResources.ToImmutableArray(),
                    CancellationToken.None,
                    out var outputCompilation,
                    out var outputResources ) )
                {
                    context.ManifestResources.Clear();
                    context.ManifestResources.AddRange( outputResources );

                    return outputCompilation;
                }
                else
                {
                    // The pipeline failed.
                    return context.Compilation;
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

                return context.Compilation;
            }
        }
    }
}