// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Telemetry;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace Metalama.Framework.Engine.Pipeline.CompileTime
{
    internal class CompileTimeExceptionHandler : ICompileTimeExceptionHandler
    {
        private readonly IExceptionReporter? _exceptionReporter;

        public CompileTimeExceptionHandler( IServiceProvider serviceProvider )
        {
            this._exceptionReporter = (IExceptionReporter?) serviceProvider.GetService( typeof(IExceptionReporter) );
        }

        public void ReportException( Exception exception, Action<Diagnostic> reportDiagnostic, out bool mustRethrow )
        {
            var reportFile = DefaultPathOptions.Instance.GetNewCrashReportPath();

            if ( reportFile != null )
            {
                File.WriteAllText( reportFile, exception.ToString() );
            }

            reportDiagnostic( GeneralDiagnosticDescriptors.UnhandledException.CreateRoslynDiagnostic( null, (exception.Message, reportFile ?? "(none)") ) );

            this._exceptionReporter?.ReportException( exception );
            
            mustRethrow = false;
        }
    }
}