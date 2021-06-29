// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Formatting
{
    public readonly struct MarkedTextSpan
    {
        public TextSpan Span { get; }

        public TextSpanClassification Classification { get; }

        public ImmutableDictionary<string, string> Tags { get; }

        internal MarkedTextSpan( in TextSpan span, TextSpanClassification classification, ImmutableDictionary<string, string>? tags )
        {
            this.Span = span;
            this.Classification = classification;
            this.Tags = tags ?? ImmutableDictionary<string, string>.Empty;
        }

        public MarkedTextSpan Update( TextSpanClassification? classification, string? name, string? value )
        {
            var newClassification = classification != null && classification.Value > this.Classification ? classification.Value : this.Classification;
            ImmutableDictionary<string, string> newTags;

            if ( name == null || (this.Tags.TryGetValue( name, out var currentTagValue ) && currentTagValue == value) )
            {
                newTags = this.Tags;
            }
            else if ( value == null )
            {
                newTags = this.Tags.Remove( name );
            }
            else
            {
                newTags = this.Tags.SetItem( name, value );
            }

            return new MarkedTextSpan( this.Span, newClassification, newTags );
        }

        public bool Satisfies( TextSpanClassification? classification, string? tagName, string? tagValue )
        {
            if ( classification != null && classification.Value > this.Classification )
            {
                return false;
            }

            if ( tagName == null )
            {
                return true;
            }
            else if ( tagValue == null )
            {
                return !this.Tags.ContainsKey( tagName );
            }
            else
            {
                return this.Tags.TryGetValue( tagName, out var currentValue ) && tagValue == currentValue;
            }
        }

        public bool HasEqualTags( ImmutableDictionary<string, string>? other )
        {
            if ( other == null )
            {
                return this.Tags.IsEmpty;
            }

            if ( this.Tags.Count != other.Count )
            {
                return false;
            }

            foreach ( var t in this.Tags )
            {
                if ( !other.TryGetValue( t.Key, out var otherValue ) || t.Value != otherValue )
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            var s = this.Span.ToString().Replace( "2147483647", "inf" ) + "=>" + this.Classification;

            foreach ( var tag in this.Tags )
            {
                s += $"{{{tag.Key}={tag.Value}}}";
            }

            return s;
        }
    }
}