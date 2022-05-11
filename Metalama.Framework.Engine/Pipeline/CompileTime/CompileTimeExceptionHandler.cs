// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Telemetry;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Metalama.Framework.Engine.Pipeline.CompileTime
{
    internal class CompileTimeExceptionHandler : ICompileTimeExceptionHandler
    {
        private readonly IExceptionReporter? _exceptionReporter;

        public CompileTimeExceptionHandler( IServiceProvider serviceProvider )
        {
            this._exceptionReporter = (IExceptionReporter?) serviceProvider.GetService( typeof(IExceptionReporter) );
        }

        public void ReportException( Exception exception, Action<Diagnostic> reportDiagnostic, bool canIgnoreException, out bool isHandled )
        {
            var reportFile = DefaultPathOptions.Instance.GetNewCrashReportPath();

            if ( reportFile != null )
            {
                var exceptionText = new StringBuilder();

                exceptionText.AppendLine( $"Metalama Version: {EngineAssemblyMetadataReader.Instance.PackageVersion}" );
                exceptionText.AppendLine( $"Runtime: {RuntimeInformation.FrameworkDescription}" );
                exceptionText.AppendLine( $"Processor Architecture: {RuntimeInformation.ProcessArchitecture}" );
                exceptionText.AppendLine( $"OS Description: {RuntimeInformation.OSDescription}" );
                exceptionText.AppendLine( $"OS Architecture: {RuntimeInformation.OSArchitecture}" );
                exceptionText.AppendLine( $"Exception type: {exception.GetType()}" );
                exceptionText.AppendLine( $"Exception message: {exception.Message}" );

                try
                {
                    // The next line may fail.
                    var exceptionToString = exception.ToString();
                    exceptionText.AppendLine( "-------" );
                    exceptionText.AppendLine( exceptionToString );
                }
                
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }

                File.WriteAllText( reportFile, exceptionText.ToString() );
            }

            var diagnosticDefinition =
                canIgnoreException ? GeneralDiagnosticDescriptors.IgnorableUnhandledException : GeneralDiagnosticDescriptors.UnhandledException;

            reportDiagnostic( diagnosticDefinition.CreateRoslynDiagnostic( null, (exception.Message, reportFile ?? "(none)") ) );

            this._exceptionReporter?.ReportException( exception );

            isHandled = true;
        }
    }
}