// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.Impl.Templating
{
    public readonly struct MarkedTextSpan
    {
        public TextSpan Span { get; }

        public TextSpanClassification Classification { get; }

        internal MarkedTextSpan( in TextSpan span, TextSpanClassification classification )
        {
            this.Span = span;
            this.Classification = classification;
        }

        public override string ToString() => this.Span.ToString().Replace( "2147483647", "inf" ) + "=>" + this.Classification;
    }
}