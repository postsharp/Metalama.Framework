namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
    internal interface IUserDiagnosticSink
    {
        /// <summary>
        /// Reports a diagnpstic.
        /// </summary>
        /// <param name="location">Location.</param>
        /// <param name="descriptor">Descriptor.</param>
        /// <param name="args">Arguments of the formatting string.</param>
        void Report( IDiagnosticLocation? location, DiagnosticDescriptor descriptor, object[] args );
    }
}