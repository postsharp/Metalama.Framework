// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Represents a <see cref="TextSpan"/> assigned to a classification.
    /// </summary>
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