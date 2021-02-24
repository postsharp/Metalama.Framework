// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.DesignTime.Contracts
{
    public readonly struct ClassifiedTextSpan
    {
        public TextSpan Span { get; }

        public TextSpanClassification Classification { get; }

        public ClassifiedTextSpan( TextSpan span, TextSpanClassification classification )
        {
            this.Span = span;
            this.Classification = classification;
        }
    }
}