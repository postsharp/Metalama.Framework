// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class CompileTimeExceptionHandler : ICompileTimeExceptionHandler
    {
        public void ReportException( Exception exception, Action<Diagnostic> reportDiagnostic, out bool mustRethrow )
        {
            var tempPath = DefaultPathOptions.Instance.CrashReportDirectory;

            RetryHelper.Retry(
                () =>
                {
                    if ( !Directory.Exists( tempPath ) )
                    {
                        Directory.CreateDirectory( tempPath );
                    }
                } );

            var reportFile = Path.Combine( tempPath, $"exception-{Guid.NewGuid()}.txt" );
            File.WriteAllText( reportFile, exception.ToString() );

            reportDiagnostic( GeneralDiagnosticDescriptors.UnhandledException.CreateDiagnostic( null, (exception.Message, reportFile) ) );
            mustRethrow = false;
        }
    }
}