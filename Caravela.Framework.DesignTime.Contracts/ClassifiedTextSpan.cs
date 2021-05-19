// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Represents a <see cref="TextSpan"/> and its <see cref="TextSpanClassification"/>.
    /// </summary>
    public readonly struct ClassifiedTextSpan
    {
        /// <summary>
        /// Gets the <see cref="TextSpan"/>.
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// Gets the classification of <see cref="Span"/>.
        /// </summary>
        public TextSpanClassification Classification { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifiedTextSpan"/> struct.
        /// </summary>
        public ClassifiedTextSpan( TextSpan span, TextSpanClassification classification )
        {
            this.Span = span;
            this.Classification = classification;
        }
    }
}