using System.Collections.Concurrent;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using DiagnosticDescriptor = Caravela.Framework.Diagnostics.DiagnosticDescriptor;
using DiagnosticSeverity = Caravela.Framework.Diagnostics.DiagnosticSeverity;
using RoslynDiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using RoslynDiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IUserDiagnosticSink"/> interface
    /// and maps user-level diagnostics into Roslyn <see cref="Diagnostic"/>.
    /// </summary>
    internal abstract class UserDiagnosticSink : IUserDiagnosticSink
    {
        private static readonly ConcurrentDictionary<DiagnosticDescriptor, RoslynDiagnosticDescriptor> _descriptors =
            new ConcurrentDictionary<DiagnosticDescriptor, RoslynDiagnosticDescriptor>();

        public void Report( IDiagnosticLocation? location, DiagnosticDescriptor descriptor, object[] args )
        {
            var roslynLocation = ((DiagnosticLocation?) location)?.Location;
            var roslynDescriptor = GetRoslynDescriptor( descriptor );
            var diagnostic = Diagnostic.Create( roslynDescriptor, roslynLocation, args );

            this.Report( diagnostic );
        }

        /// <summary>
        /// Reports a <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="diagnostic"></param>
        protected abstract void Report( Diagnostic diagnostic );

        private static RoslynDiagnosticDescriptor GetRoslynDescriptor( DiagnosticDescriptor descriptor )
            => _descriptors.GetOrAdd( descriptor,
                d => new RoslynDiagnosticDescriptor(
                    d.Id, d.Id, d.MessageFormat, "Caravela.User", MapSeverity( d.Severity ), true ) );

        private static RoslynDiagnosticSeverity MapSeverity( DiagnosticSeverity severity ) =>
            severity switch
            {
                DiagnosticSeverity.Error => RoslynDiagnosticSeverity.Error,
                DiagnosticSeverity.Hidden => RoslynDiagnosticSeverity.Hidden,
                DiagnosticSeverity.Info => RoslynDiagnosticSeverity.Info,
                DiagnosticSeverity.Warning => RoslynDiagnosticSeverity.Warning,
                _ => throw new AssertionFailedException()
            };
    }
}