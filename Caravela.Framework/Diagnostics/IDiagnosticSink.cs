// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Project;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
    [CompileTimeOnly]
    public interface IDiagnosticSink
    {

        /// <summary>
        /// Reports a diagnostic by specifying its location.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="location">The code location to which the diagnostic should be written.</param>
        /// <param name="id"></param>
        /// <param name="formatMessage"></param>
        /// <param name="args">Arguments of the formatting string.</param>
        void ReportDiagnostic( Severity severity, IDiagnosticLocation location, string id, string formatMessage, params object[] args );

        /// <summary>
        /// Reports a diagnostic and uses the location of the current target.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="id"></param>
        /// <param name="formatMessage"></param>
        /// <param name="args">Arguments of the formatting string.</param>
        void ReportDiagnostic( Severity severity, string id, string formatMessage, params object[] args );

        /// <summary>
        /// Suppresses a diagnostic by specifying the element of code in which the suppression must be effective.
        /// </summary>
        /// <param name="id">The id of the identifier to suppress.</param>
        /// <param name="scope">The code element in which the diagnostic must be suppressed.</param>
        void SuppressDiagnostic( string id, ICodeElement scope );

        /// <summary>
        /// Suppresses a diagnostic in the current target declaration.
        /// </summary>
        /// <param name="id"></param>
        void SuppressDiagnostic( string id );
    }
}