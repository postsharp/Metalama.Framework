// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.ServiceProvider;
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
        public void Execute( TransformerContext context )
        {
            var projectOptions = new ProjectOptions( context.GlobalOptions, context.Plugins );

            try
            {
                using CompileTimeAspectPipeline pipeline = new(
                    projectOptions,
                    false,
                    assemblyLocator: new CompilationAssemblyLocator( context.Compilation ) );

                if ( pipeline.TryExecute(
                    new DiagnosticAdderAdapter( context.ReportDiagnostic ),
                    context.Compilation,
                    context.Resources.ToImmutableArray(),
                    CancellationToken.None,
                    out var syntaxTreeTransformations,
                    out var additionalResources,
                    out _ ) )
                {
                    context.AddResources( additionalResources );
                    context.AddSyntaxTreeTransformations( syntaxTreeTransformations );
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