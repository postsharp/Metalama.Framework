// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Diagnostics
{
    public static class DiagnosticSinkExtensions
    {
        /// <summary>
        /// Reports a diagnostic by specifying its target declaration.
        /// </summary>
        /// <param name="diagnosticSink"></param> 
        /// <param name="severity"></param>
        /// <param name="scope">The target declaration of the diagnostic (typically an <see cref="ICodeElement"/>). If null, the location of the current target is used. </param>
        /// <param name="id"></param>
        /// <param name="formatMessage"></param>
        /// <param name="args">Arguments of the formatting string.</param>
        public static void ReportDiagnostic(
            this IDiagnosticSink diagnosticSink,
            Severity severity,
            IDiagnosticScope scope,
            string id,
            string formatMessage,
            params object[] args )
        {
            if ( scope.LocationForDiagnosticReport != null )
            {
                diagnosticSink.ReportDiagnostic( severity, scope.LocationForDiagnosticReport, id, formatMessage, args );
            }
        }
    }
}