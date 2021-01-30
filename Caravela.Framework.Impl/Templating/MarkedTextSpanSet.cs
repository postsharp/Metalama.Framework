using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Text;

namespace Caravela.Framework.Impl.Templating
{
    public interface IMarkedTextSpanSet
    {
        TextSpanCategory GetCategory( in TextSpan textSpan );
    }
    
    /// <summary>
    /// A set of <see cref="TextSpan"/>. 
    /// </summary>
    sealed class MarkedTextSpanSet : IMarkedTextSpanSet
    {
        
        
        private readonly SkipListIndexedDictionary<int, MarkedTextSpan> _spans = new SkipListIndexedDictionary<int, MarkedTextSpan>();

        public MarkedTextSpanSet()
        {
            // Start with a single default span. This avoid gaps in the partition later.
            this._spans.Add( 0, new MarkedTextSpan( new TextSpan( 0, int.MaxValue ), TextSpanCategory.Default ) );
        }

        /// <summary>
        /// Adds a marked <see cref="TextSpan"/>.
        /// </summary>
        /// <param name="span"></param>
        internal void Mark( in TextSpan span, TextSpanCategory category )
        {
            for ( int i = 0; ;i++ )
            {
                if ( i > 4 )
                    throw new AssertionFailedException();
                    
                if ( this._spans.TryGetClosestValue( span.Start, out var previousStartSpan ) && previousStartSpan.Span.IntersectsWith( span ) )
                {
                    // Check if we have an exact span. If not, we will partition.
                    if ( previousStartSpan.Span.Equals( span )  )
                    {
                        // We have an exact span, so no need to partition.
                        
                        if ( previousStartSpan.Category < category )
                        {
                            // The new span contains the old one and is stronger, so we replace the new one 
                            // by the old one.
                            this._spans.Set( span.Start, new MarkedTextSpan( span, category ) );
                        }

                        return;
                    }

                    // We need to partition the existing spans to the new span boundaries.
                    if ( span.Start == previousStartSpan.Span.Start )
                    {
                        // We have hit an existing span start. Check if we need to partition the end.
                        if ( this._spans.TryGetClosestValue( span.End, out var previousEndSpan ) && previousEndSpan.Span.End != span.End && previousEndSpan.Span.Start != span.End )
                        {
                            // We have to split the existing span that conflicts with the end of the new span.
                            var (a, b) = Split( previousEndSpan, span.End );
                            this._spans.Set( a.Span.Start, a );
                            this._spans.Add( b.Span.Start, b );

                        }
                        else
                        {
                            // We have a partition of existing spans than matches the boundaries of the new span.
                            // Check all

                            foreach ( var pair in this._spans.GetItemsGreaterOrEqualThan( span.Start ) )
                            {
                                if ( pair.Value.Span.Start >= span.End )
                                {
                                    break;
                                }
                                
                                if ( pair.Value.Category < category )
                                {
                                    // Replace the category of this node.
                                    this._spans.Set( pair.Key, new MarkedTextSpan( pair.Value.Span, category ) );
                                }
                            }

                            return;
                        }
                    }
                    else
                    {
                        // The situation is more complex. We will split the old span in a partition so
                        // that we are back to a simple situation.

                        var (a, b) = Split( previousStartSpan, span.Start );
                        this._spans.Set( a.Span.Start, a );

                        if ( b.Span.Contains( span.End ) )
                        {
                            var (c, d) = Split( b, span.End );
                            this._spans.Add( c.Span.Start, c );
                            this._spans.Add( d.Span.Start, d );
                        }
                        else
                        {
                            this._spans.Add( b.Span.Start, b );
                        }
                    }

                    
                }
                else if ( this._spans.TryGetClosestValue( span.End, out var previousEndSpan ) && previousEndSpan.Span.IntersectsWith( span ) )
                {
                    // An existing span intersects with the end. Split it and retry.

                    var (a, b) = Split( previousStartSpan, span.Start );
                    this._spans.Set( a.Span.Start, a );
                    this._spans.Add( b.Span.Start, b );

                }
                else
                {
                    // We cannot get here because we start with a whole partition.
                    throw new AssertionFailedException();
                }

               
            }
        }

        private static ( MarkedTextSpan, MarkedTextSpan ) Split( in MarkedTextSpan textSpan, int splitPosition )
        {
            if ( !textSpan.Span.Contains( splitPosition ) || splitPosition == textSpan.Span.Start )
                throw new ArgumentException( nameof(splitPosition) );
            
            return (new MarkedTextSpan( TextSpan.FromBounds(  textSpan.Span.Start, splitPosition ), textSpan.Category ),
                new MarkedTextSpan( TextSpan.FromBounds(  splitPosition, textSpan.Span.End ),
                    textSpan.Category ));
        }


        public TextSpanCategory GetCategory( in TextSpan textSpan )
        {
            if ( this._spans.TryGetClosestValue( textSpan.Start, out var markedTextSpan ))
            {
                if ( markedTextSpan.Span.Contains( textSpan ) )
                {
                    return markedTextSpan.Category;
                }
                else
                {
                    return TextSpanCategory.Conflict;
                }
                
            }
            else
            {
                // This should not happen because our partition covers [0,int.MaxValue]
                return TextSpanCategory.Default;
            }
        }

        
        public override string ToString()
        {
            // Used for unit testing. Don't change the rendering logic.
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append( "{ " );
            foreach ( var pair in this._spans )
            {
                if ( stringBuilder.Length > 2 )
                {
                    stringBuilder.Append( ", " );
                }

                stringBuilder.Append( pair.Value.ToString() );
            }
            stringBuilder.Append( " } ");

            return stringBuilder.ToString();
        }
    }

    public readonly struct MarkedTextSpan
    {
        public TextSpan Span { get; }
        public TextSpanCategory Category { get; }

        internal MarkedTextSpan( in TextSpan span, TextSpanCategory category )
        {
            this.Span = span;
            this.Category = category;
        }

        public override string ToString() => this.Span.ToString().Replace( "2147483647", "inf" ) + "=>" + this.Category;
    }

    public enum TextSpanCategory
    {
        // Order of declaration (or at last enum value) matters. The higher value overwrites the lower.
        Default,
        CompileTime,
        Dynamic,
        Variable,
        Keyword,
        Conflict // A text span has several categories.
    }
}