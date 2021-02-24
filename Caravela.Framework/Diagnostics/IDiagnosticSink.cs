// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
    [CompileTime]
    public interface IDiagnosticSink
    {

        /// <summary>
        /// Reports a diagnostic by specifying its location.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="location">Location.</param>
        /// <param name="id"></param>
        /// <param name="formatMessage"></param>
        /// <param name="args">Arguments of the formatting string.</param>
        void ReportDiagnostic( Severity severity, IDiagnosticLocation? location, string id, string formatMessage, params object[] args );

        /// <summary>
        /// Reports a diagnostic and uses the default location for the current context.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="id"></param>
        /// <param name="formatMessage"></param>
        /// <param name="args">Arguments of the formatting string.</param>
        void ReportDiagnostic( Severity severity, string id, string formatMessage, params object[] args );
    }
}