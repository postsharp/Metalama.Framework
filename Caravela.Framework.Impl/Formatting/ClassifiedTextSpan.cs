// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Formatting
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

        public ImmutableDictionary<string, string> Tags { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifiedTextSpan"/> struct.
        /// </summary>
        public ClassifiedTextSpan( TextSpan span, TextSpanClassification classification, ImmutableDictionary<string, string>? tags )
        {
            this.Span = span;
            this.Classification = classification;
            this.Tags = tags ?? ImmutableDictionary<string, string>.Empty;
        }
    }
}