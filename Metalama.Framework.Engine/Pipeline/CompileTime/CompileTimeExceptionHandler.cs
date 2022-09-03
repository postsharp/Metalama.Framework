// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
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
        private readonly ITempFileManager _tempFileManager;

        public CompileTimeExceptionHandler( IServiceProvider serviceProvider )
        {
            this._exceptionReporter = (IExceptionReporter?) serviceProvider.GetService( typeof(IExceptionReporter) );
            this._tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();
        }

        public void ReportException( Exception exception, Action<Diagnostic> reportDiagnostic, bool canIgnoreException, out bool isHandled )
        {
            // Unwrap AggregateException.
            if ( exception is AggregateException { InnerExceptions: { Count: 1 } innerExceptions } )
            {
                exception = innerExceptions[0];
            }

            SyntaxNode? node = null;

            if ( exception is SyntaxProcessingException syntaxProcessingException )
            {
                node = syntaxProcessingException.SyntaxNode;
            }

            var reportFile = Path.Combine(
                this._tempFileManager.GetTempDirectory( "CrashReports", CleanUpStrategy.Always ),
                $"exception-{Guid.NewGuid()}.txt" );

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
                exceptionText.AppendLine( "===== Exception ===== " );
                exceptionText.AppendLine( exceptionToString );

                if ( node != null )
                {
                    exceptionText.AppendLine( "===== Syntax Tree ===== " );
                    exceptionText.AppendLine( node.SyntaxTree.GetText().ToString() );
                }
            }

            // ReSharper disable once EmptyGeneralCatchClause
            catch { }

            File.WriteAllText( reportFile, exceptionText.ToString() );

            var diagnosticDefinition =
                canIgnoreException ? GeneralDiagnosticDescriptors.IgnorableUnhandledException : GeneralDiagnosticDescriptors.UnhandledException;

            reportDiagnostic( diagnosticDefinition.CreateRoslynDiagnostic( node?.GetLocation(), (exception.Message, reportFile) ) );

            this._exceptionReporter?.ReportException( exception );

            isHandled = true;
        }
    }
}