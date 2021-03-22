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
            IDiagnosticScope? scope,
            string id,
            string formatMessage,
            params object[] args )
            => diagnosticSink.ReportDiagnostic( severity, scope?.LocationForDiagnosticReport, id, formatMessage, args );

        /// <summary>
        /// Suppresses a diagnostic by specifying the declaration in which the suppression should be effective.
        /// </summary>
        /// <param name="diagnosticSink"></param>
        /// <param name="id">The id of the identifier to suppress.</param>
        /// <param name="scope">The declaration (typically an <see cref="ICodeElement"/>) in which the diagnostic must be suppressed.</param>
        public static void SuppressDiagnostic( this IDiagnosticSink diagnosticSink, string id, IDiagnosticScope scope )
        {
            foreach ( var location in scope.LocationsForDiagnosticSuppression )
            {
                diagnosticSink.SuppressDiagnostic( id, location );
            }
        }
    }
}