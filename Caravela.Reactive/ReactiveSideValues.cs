using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Reactive
{
    /// <summary>
    /// An immutable list of side values.
    /// Side values are guaranteed to be copied (combined), from source to result, for all operators.
    /// </summary>
    public readonly struct ReactiveSideValues
    {
        private readonly ImmutableArray<IReactiveSideValue> _sideValues;

        IReadOnlyList<IReactiveSideValue> SideValues => this._sideValues;

        private ReactiveSideValues( ImmutableArray<IReactiveSideValue> sideValues )
        {
            this._sideValues = sideValues;
        }

        public static ReactiveSideValues Create( IReactiveSideValue sideValue ) => new ReactiveSideValues( ImmutableArray.Create( sideValue ) );

        public ReactiveSideValues WithSideValue( IReactiveSideValue value )
        {
            if ( this._sideValues.IsDefaultOrEmpty )
            {
                return new ReactiveSideValues( ImmutableArray.Create( value ) );
            }
            else
            {
                return new ReactiveSideValues( this._sideValues.Add( value ) );
            }
        }

        /// <summary>
        /// Combines the current side values (typically stemming from the source) with other side values (typically coming from the valuation of the
        /// function or its dependencies).
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public ReactiveSideValues Combine( ReactiveSideValues other )
        {
            if ( this.SideValues == null )
            {
                return other;
            }
            else if ( other.SideValues == null )
            {
                return this;
            }
            else
            {
                // There's typically just one item so this is optimized for this situation.

                var builder = ImmutableArray.CreateBuilder<IReactiveSideValue>( this.SideValues.Count );
                builder.AddRange( this.SideValues );
                foreach ( var otherValue in other.SideValues )
                {
                    for ( int i = 0; i < builder.Count; i++ )
                    {
                        if ( builder[i].TryCombine( otherValue, out var combinedValue ) )
                        {
                            builder[i] = combinedValue;
                            continue;
                        }
                    }

                    // We could not combine, so we append it.
                    builder.Add( otherValue );

                }

                return new ReactiveSideValues( builder.ToImmutable() );
            }
        }
    }
}