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

        /// <summary>
        /// Gets the <see cref="IUserDiagnosticSink"/> for the current execution context.
        /// </summary>
        public IUserDiagnosticSink? Sink { get; }

        /// <summary>
        /// Gets the default target, used when the location or target is not specified by the user.
        /// </summary>
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
        public static IDisposable WithContext( IUserDiagnosticSink? sink, IDiagnosticTarget? defaultTarget )
        {
            var cookie = new Cookie( _current.Value );
            _current.Value = new( sink, defaultTarget );
            return cookie;
        }

        /// <summary>
        /// Sets the <see cref="IUserDiagnosticSink"/> for the current execution context,
        /// and returns an opaque <see cref="IDisposable"/> that the caller must dispose to return to the previous execution context data.
        /// </summary>
        /// <param name="sink"></param>
        /// <returns></returns>
        public static IDisposable WithSink( IUserDiagnosticSink? sink ) => WithContext( sink, Current.DefaultTarget );

        /// <summary>
        /// Sets the default target, used when the location or target is not specified by the user, for the current execution context,
        /// and returns an opaque <see cref="IDisposable"/> that the caller must dispose to return to the previous execution context data.
        /// </summary>
        /// <param name="defaultTarget"></param>
        /// <returns></returns>
        public static IDisposable WithDefaultTarget( IDiagnosticTarget? defaultTarget ) => WithContext( Current.Sink, defaultTarget );

        private class Cookie : IDisposable
        {
            private readonly DiagnosticContext _previousValue;

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