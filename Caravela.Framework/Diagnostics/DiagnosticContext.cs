// unset

using System;
using System.Threading;

namespace Caravela.Framework.Diagnostics
{
    internal readonly struct DiagnosticContext
    {
        private static readonly AsyncLocal<DiagnosticContext> _current = new();
        public static DiagnosticContext Current => _current.Value;
        public IDiagnosticSink? Sink { get; }
        public IDiagnosticTarget? DefaultTarget { get; }

        public DiagnosticContext( IDiagnosticSink? sink, IDiagnosticTarget? defaultTarget )
        {
            this.Sink = sink;
            this.DefaultTarget = defaultTarget;
        }

        public static IDisposable With( IDiagnosticSink? sink, IDiagnosticTarget? defaultTarget )
        {
            var cookie = new Cookie( _current.Value );
            _current.Value = new(sink, defaultTarget);
            return cookie;
        }


        class Cookie : IDisposable
        {
            private DiagnosticContext _previousValue;

            public Cookie( DiagnosticContext previousValue )
            {
                this._previousValue = previousValue;
            }

            public void Dispose()
            {
                _current.Value = this._previousValue;
            }
        }
    }
}