// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace Metalama.Framework.Engine.Pipeline
{
    internal class CompileTimeExceptionHandler : ICompileTimeExceptionHandler
    {
        public void ReportException( Exception exception, Action<Diagnostic> reportDiagnostic, out bool mustRethrow )
        {
            var reportFile = DefaultPathOptions.Instance.GetNewCrashReportPath();

            if ( reportFile != null )
            {
                File.WriteAllText( reportFile, exception.ToString() );
            }

            reportDiagnostic( GeneralDiagnosticDescriptors.UnhandledException.CreateDiagnostic( null, (exception.Message, reportFile ?? "(none)") ) );
            mustRethrow = false;
        }
    }
}