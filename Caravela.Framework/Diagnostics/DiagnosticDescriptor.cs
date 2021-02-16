using Caravela.Framework.Code;

namespace Caravela.Framework.Diagnostics
{
    public sealed class DiagnosticDescriptor
    {
        public string Id { get; }
        public string MessageFormat { get; }
        public DiagnosticSeverity Severity { get; }

        public void Report( IDiagnosticTarget target, params object[] args )
        {
            var diagnosticContext = DiagnosticContext.Current;
            diagnosticContext.Sink?.Report( target?.DiagnosticLocation ?? diagnosticContext.DefaultTarget?.DiagnosticLocation, this, args );
        }
        
        public void Report( IDiagnosticLocation location, params object[] args )
        {
            var diagnosticContext = DiagnosticContext.Current;
            diagnosticContext.Sink?.Report( location ?? diagnosticContext.DefaultTarget?.DiagnosticLocation, this, args );
        }
        
        public void Report( params object[] args )
        {
            var diagnosticContext = DiagnosticContext.Current;
            diagnosticContext.Sink?.Report( diagnosticContext.DefaultTarget?.DiagnosticLocation, this, args );
        }
    }
}