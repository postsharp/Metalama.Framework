using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// A set of <see cref="TextSpan"/>. 
    /// </summary>
    sealed class MarkedSpanSet
    {
        // This is a quick and naive implementation. This algorithm is actually quite exactly the interview
        // test for the company so I guess we can do better.
        private readonly Dictionary<int, TextSpan> _spans = new Dictionary<int, TextSpan>();

        /// <summary>
        /// Adds a marked <see cref="TextSpan"/>.
        /// </summary>
        /// <param name="span"></param>
        internal void Mark( in TextSpan span )
        {
            if ( !this._spans.TryGetValue( span.Start, out var existingSpan ) || existingSpan.End < span.End )
            {
                this._spans[span.Start] = span;
            }
        }

        public ImmutableList<TextSpan> GetMarkedSpans() => this._spans.Values.OrderBy( span => span.Start ).ToImmutableList();


    }
}