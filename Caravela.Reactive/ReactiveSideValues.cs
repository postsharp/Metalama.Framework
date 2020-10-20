using System;
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

        public static ReactiveSideValues Create( IReactiveSideValue? sideValue ) => sideValue == null ? default : new ReactiveSideValues( ImmutableArray.Create( sideValue ) );

        
        public bool TryGetValue<T>(out T? value) where T : class, IReactiveSideValue
        {
            if (this._sideValues.IsDefaultOrEmpty )
            {
                value = null;
                return false;
            }

            foreach ( var item in this._sideValues )
            {
                if ( item is T t )
                {
                    value = t;
                    return true;
                }
            }

            value = null;
            return false;
        }

        ImmutableArray<IReactiveSideValue>.Builder CreateBuilder()
        {
            // There's typically just one item so this is optimized for this situation.
            var builder = ImmutableArray.CreateBuilder<IReactiveSideValue>( this.SideValues.Count );
            builder.AddRange( this.SideValues );
            return builder;
        }

        void Combine(ref ImmutableArray<IReactiveSideValue>.Builder builder, IReactiveSideValue value )
        {
            for ( int i = 0; i < builder.Count; i++ )
            {
                if ( builder[i].TryCombine( value, out var combinedValue ) )
                {
                    builder[i] = combinedValue;
                    return;
                }
            }

            // We could not combine, so we append it.
            builder.Add( value );

        }


        public ReactiveSideValues Combine( IReactiveSideValue value )
        {
            if ( this._sideValues.IsDefaultOrEmpty )
            {
                // Quick path.
                return new ReactiveSideValues( ImmutableArray.Create( value ) );
            }
            else
            {
                var builder = this.CreateBuilder();
                this.Combine( ref builder, value );

                return new ReactiveSideValues( builder.MoveToImmutable() );
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
                var builder = this.CreateBuilder();
                foreach ( var otherValue in other.SideValues )
                {
                    this.Combine( ref builder, otherValue );
                }

                return new ReactiveSideValues( builder.MoveToImmutable() );
            }
        }
    }
}