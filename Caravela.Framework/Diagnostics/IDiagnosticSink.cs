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
        /// <param name="location">Location</param>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="args">Arguments of the formatting string</param>
        void ReportDiagnostic( Severity severity, IDiagnosticLocation? location, string id, string formatMessage, params object[] args );

        /// <summary>
        /// Reports a diagnostic and uses the default location for the current context.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="id"></param>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        void ReportDiagnostic( Severity severity, string id, string formatMessage, params object[] args );
    }
}