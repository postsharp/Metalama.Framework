// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.VisualStudio.Classification;

internal class DesignTimeClassifiedTextSpansCollection : IDesignTimeClassifiedTextCollection
{
    private readonly ClassifiedTextSpanCollection _underlying;

    public DesignTimeClassifiedTextSpansCollection( ClassifiedTextSpanCollection underlying )
    {
        this._underlying = underlying;
    }

    public IEnumerable<DesignTimeClassifiedTextSpan> GetClassifiedTextSpans()
        => this._underlying.Select( x => new DesignTimeClassifiedTextSpan( x.Span, x.Classification.ToDesignTime() ) );

    public IEnumerable<DesignTimeClassifiedTextSpan> GetClassifiedTextSpans( TextSpan textSpan )
        => this._underlying.GetClassifiedSpans( textSpan ).Select( x => new DesignTimeClassifiedTextSpan( x.Span, x.Classification.ToDesignTime() ) );
}