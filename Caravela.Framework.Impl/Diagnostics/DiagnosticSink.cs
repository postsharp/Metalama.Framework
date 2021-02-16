using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DiagnosticDescriptor = Caravela.Framework.Diagnostics.DiagnosticDescriptor;
using DiagnosticSeverity = Caravela.Framework.Diagnostics.DiagnosticSeverity;
using IDiagnosticSink = Caravela.Framework.Sdk.IDiagnosticSink;
using RoslynDiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using RoslynDiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IUserDiagnosticSink"/> interface
    /// and bridges it into an SDK-level <see cref="IDiagnosticSink"/>.
    /// </summary>
    public abstract class UserUserDiagnosticSink : Caravela.Framework.Diagnostics.IUserDiagnosticSink
    {
        private static ConcurrentDictionary<DiagnosticDescriptor, RoslynDiagnosticDescriptor> _descriptors =
            new ConcurrentDictionary<DiagnosticDescriptor, RoslynDiagnosticDescriptor>();

        public void Report( IDiagnosticLocation? location, DiagnosticDescriptor descriptor, object[] args )
        {
            var roslynLocation = ((RoslynDiagnosticLocation?) location)?.Location;
            var roslynDescriptor = GetRoslynDescriptor( descriptor );
            var diagnostic = Diagnostic.Create( roslynDescriptor, roslynLocation, args );

            this.Report( diagnostic );

        }

        protected abstract void Report( Diagnostic diagnostic );

        static RoslynDiagnosticDescriptor GetRoslynDescriptor( DiagnosticDescriptor descriptor )
            => _descriptors.GetOrAdd( descriptor,
                d => new RoslynDiagnosticDescriptor(
                    d.Id, d.Id, d.MessageFormat, "Caravela.User", MapSeverity( d.Severity ), true ) );

        static RoslynDiagnosticSeverity MapSeverity( DiagnosticSeverity severity ) =>
            severity switch
            {
                DiagnosticSeverity.Error => RoslynDiagnosticSeverity.Error,
                DiagnosticSeverity.Hidden => RoslynDiagnosticSeverity.Hidden,
                DiagnosticSeverity.Info => RoslynDiagnosticSeverity.Info,
                DiagnosticSeverity.Warning => RoslynDiagnosticSeverity.Warning,
                _ => throw new AssertionFailedException()
            };
    }

    public class UserDiagnosticList : UserUserDiagnosticSink, IReadOnlyList<Diagnostic>
    {
        
        private List<Diagnostic>? _diagnostics;
        
        protected override void Report( Diagnostic diagnostic )
        {
            this._diagnostics ??= new List<Diagnostic>();
            this._diagnostics.Add( diagnostic );
        }

        public IEnumerator<Diagnostic> GetEnumerator() => this._diagnostics?.GetEnumerator() ?? Enumerable.Empty<Diagnostic>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this._diagnostics?.Count ?? 0;

        public Diagnostic this[ int index ] => this._diagnostics.AssertNotNull()[index];
    }

    public class UserDiagnosticSinkBridge : UserUserDiagnosticSink
    { 
        private IDiagnosticSink _sink;
        
        public UserDiagnosticSinkBridge( IDiagnosticSink sink )
        {
            this._sink = sink;
        }


        protected override void Report( Diagnostic diagnostic ) => this._sink.AddDiagnostic( diagnostic );
    }
}