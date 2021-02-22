using Caravela.Framework.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// An implementation of <see cref="IUserDiagnosticSink"/> that writes into a <see cref="IDiagnosticSink"/>.
    /// </summary>
    internal class UserDiagnosticSinkBridge : UserDiagnosticSink
    {
        private readonly IDiagnosticSink _sink;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDiagnosticSinkBridge"/> class.
        /// </summary>
        /// <param name="sink"></param>
        public UserDiagnosticSinkBridge( IDiagnosticSink sink )
        {
            this._sink = sink;
        }

        /// <inheritdoc/>
        protected override void Report( Diagnostic diagnostic ) => this._sink.AddDiagnostic( diagnostic );
    }
}