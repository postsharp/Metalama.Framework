namespace Caravela.Framework.Diagnostics
{
    internal interface IDiagnosticSink
    {
        void Report( IDiagnosticLocation? location, DiagnosticDescriptor descriptor, object[] args );
    }
}