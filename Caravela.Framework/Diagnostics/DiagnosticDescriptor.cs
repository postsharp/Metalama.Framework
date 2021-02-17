namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    ///  Reprensents the metadata of a user-code diagnostic.
    /// </summary>
    public sealed class DiagnosticDescriptor
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticDescriptor"/> class.
        /// </summary>
        /// <param name="id">Diagnostic identifier, for instance <c>JB007</c>.</param>
        /// <param name="severity">Severity.</param>
        /// <param name="messageFormat">A formatting string containing placeholders like <c>{0}</c> for parameters.</param>
        public DiagnosticDescriptor( string id, DiagnosticSeverity severity, string messageFormat )
        {
            this.Id = id;
            this.MessageFormat = messageFormat;
            this.Severity = severity;
        }

        /// <summary>
        /// Gets the diagnostic identifier.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the message formatting string.
        /// </summary>
        public string MessageFormat { get; }
        
        /// <summary>
        /// Gets the diagnostics severity.
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Reports an instance of the current <see cref="DiagnosticDescriptor"/> for a given target. 
        /// </summary>
        /// <param name="target">The target code element, to which the diagnostic file, line and column should be resolved.</param>
        /// <param name="args">Arguments of <see cref="MessageFormat"/>.</param>
        public void Report( IDiagnosticTarget target, params object[] args )
        {
            var diagnosticContext = DiagnosticContext.Current;
            diagnosticContext.Sink?.Report( target?.DiagnosticLocation ?? diagnosticContext.DefaultTarget?.DiagnosticLocation, this, args );
        }
        
        /// <summary>
        /// Reports an instance of the current <see cref="DiagnosticDescriptor"/> for a given location. 
        /// </summary>
        /// <param name="location">The location to which the diagnostic file, line and column should be resolved.</param>
        /// <param name="args">Arguments of <see cref="MessageFormat"/>.</param>
        public void Report( IDiagnosticLocation location, params object[] args )
        {
            var diagnosticContext = DiagnosticContext.Current;
            diagnosticContext.Sink?.Report( location, this, args );
        }
        
        /// <summary>
        /// Reports an instance of the current <see cref="DiagnosticDescriptor"/> for the default target in the current execution context.
        /// </summary>
        /// <param name="args">Arguments of <see cref="MessageFormat"/>.</param>
        public void Report( params object[] args )
        {
            var diagnosticContext = DiagnosticContext.Current;
            diagnosticContext.Sink?.Report( diagnosticContext.DefaultTarget?.DiagnosticLocation, this, args );
        }
    }
}