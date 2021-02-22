using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// A list of <see cref="Diagnostic"/> that implements <see cref="IUserDiagnosticSink"/>.
    /// </summary>
    internal class UserDiagnosticList : UserDiagnosticSink, IReadOnlyList<Diagnostic>
    {

        private List<Diagnostic>? _diagnostics;

        /// <inheritdoc/>
        protected override void Report( Diagnostic diagnostic )
        {
            this._diagnostics ??= new List<Diagnostic>();
            this._diagnostics.Add( diagnostic );
        }

        public IEnumerator<Diagnostic> GetEnumerator() => this._diagnostics?.GetEnumerator() ?? Enumerable.Empty<Diagnostic>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this._diagnostics?.Count ?? 0;

        public Diagnostic this[int index] => this._diagnostics.AssertNotNull()[index];
    }
}