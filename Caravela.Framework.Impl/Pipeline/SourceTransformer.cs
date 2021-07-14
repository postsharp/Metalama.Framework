// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    public sealed class SourceTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext context )
        {
            var projectOptions = new ProjectOptions( context.GlobalOptions, context.Plugins );

            try
            {
                using CompileTimeAspectPipeline pipeline = new(
                    projectOptions,
                    new CompileTimeDomain(),
                    false,
                    null,
                    new CompilationAssemblyLocator( context.Compilation ) );

                if ( pipeline.TryExecute(
                    new DiagnosticAdder( context.ReportDiagnostic ),
                    context.Compilation,
                    CancellationToken.None,
                    out var compilation,
                    out var additionalResources ) )
                {
                    context.ManifestResources.AddRange( additionalResources );

                    return compilation;
                }
                else
                {
                    // The pipeline failed.
                    return context.Compilation;
                }
            }
            catch ( Exception e )
            {
                var tempPath = DefaultDirectoryOptions.Instance.CrashReportDirectory;

                RetryHelper.Retry(
                    () =>
                    {
                        if ( !Directory.Exists( tempPath ) )
                        {
                            Directory.CreateDirectory( tempPath );
                        }
                    } );

                var reportFile = Path.Combine( tempPath, $"exception-{Guid.NewGuid()}.txt" );
                File.WriteAllText( reportFile, e.ToString() );

                context.ReportDiagnostic( GeneralDiagnosticDescriptors.UnhandledException.CreateDiagnostic( null, (e.Message, reportFile) ) );

                return context.Compilation;
            }
        }
    }
}