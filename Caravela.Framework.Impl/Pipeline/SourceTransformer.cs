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
        public Compilation Execute( TransformerContext transformerContext )
        {
            var projectOptions = new ProjectOptions( transformerContext.GlobalOptions, transformerContext.Plugins );

            try
            {
                using CompileTimeAspectPipeline pipeline = new(
                    projectOptions,
                    new CompileTimeDomain(),
                    false,
                    null,
                    new CompilationAssemblyLocator( transformerContext.Compilation ) );

                if ( pipeline.TryExecute(
                    new DiagnosticAdder( transformerContext.ReportDiagnostic ),
                    transformerContext.Compilation,
                    CancellationToken.None,
                    out var compilation,
                    out var additionalResources ) )
                {
                    transformerContext.ManifestResources.AddRange( additionalResources );

                    return compilation;
                }
                else
                {
                    // The pipeline failed.
                    return transformerContext.Compilation;
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

                transformerContext.ReportDiagnostic( GeneralDiagnosticDescriptors.UnhandledException.CreateDiagnostic( null, (e.Message, reportFile) ) );

                return transformerContext.Compilation;
            }
        }
    }
}