// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.Contracts
{
    public readonly struct DesignTimeClassifiedTextSpan
    {
        /// <summary>
        /// Gets the <see cref="TextSpan"/>.
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// Gets the classification of <see cref="Span"/>.
        /// </summary>
        public DesignTimeTextSpanClassification Classification { get; }

        public DesignTimeClassifiedTextSpan( TextSpan span, DesignTimeTextSpanClassification classification )
        {
            this.Span = span;
            this.Classification = classification;
        }
    }
}