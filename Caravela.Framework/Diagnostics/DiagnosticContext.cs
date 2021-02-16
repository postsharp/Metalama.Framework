using System;
using System.Threading;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// Manages the execution context data for diagnostics. 
    /// </summary>
    internal readonly struct DiagnosticContext
    {
        private static readonly AsyncLocal<DiagnosticContext> _current = new();
        public static DiagnosticContext Current => _current.Value;
        public IUserDiagnosticSink? Sink { get; }
        public IDiagnosticTarget? DefaultTarget { get; }

        private DiagnosticContext( IUserDiagnosticSink? sink, IDiagnosticTarget? defaultTarget )
        {
            this.Sink = sink;
            this.DefaultTarget = defaultTarget;
        }

        
        /// <summary>
        /// Changes the diagnostic data for the current execution context.
        /// </summary>
        /// <param name="sink">The current sink.</param>
        /// <param name="defaultTarget">The default target, used when the location or target is not specified by the user.</param>
        /// <returns>An opaque <see cref="IDisposable"/> that the caller must dispose to return to the previous execution context data.</returns>
        public static IDisposable With( IUserDiagnosticSink? sink, IDiagnosticTarget? defaultTarget )
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