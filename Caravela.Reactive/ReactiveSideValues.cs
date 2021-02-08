using System;
using System.Collections.Immutable;

namespace Caravela.Reactive
{
    /// <summary>
    /// An immutable list of side values.
    /// Side values are guaranteed to be copied (combined), from source to result, for all operators.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By design, there can be only one side value of each <see cref="Type"/> in each <see cref="ReactiveSideValues"/>
    /// instance. This avoids the use of value names.
    /// </para>
    /// </remarks>
    public readonly struct ReactiveSideValues
    {
        private readonly ImmutableArray<IReactiveSideValue> _sideValues;

        private ReactiveSideValues( ImmutableArray<IReactiveSideValue> sideValues )
        {
            this._sideValues = sideValues;
        }

        /// <summary>
        /// Creates a <see cref="ReactiveSideValues"/> object from a single <see cref="IReactiveSideValue"/>.
        /// </summary>
        /// <param name="sideValue"></param>
        /// <returns></returns>
        public static ReactiveSideValues Create( IReactiveSideValue? sideValue ) => sideValue == null ? default : new ReactiveSideValues( ImmutableArray.Create( sideValue ) );

        /// <summary>
        /// Gets the side value of a given type from the current instance.
        /// </summary>
        /// <param name="value">At output, the side value, or <c>null</c> if there is no side value of type <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The type of the side value to get.</typeparam>
        /// <returns><c>true</c> if a side value was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue<T>( out T? value )
            where T : class, IReactiveSideValue
        {
            if ( this._sideValues.IsDefaultOrEmpty )
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

        private ImmutableArray<IReactiveSideValue>.Builder CreateBuilder()
        {
            // There's typically just one item so this is optimized for this situation.
            var builder = ImmutableArray.CreateBuilder<IReactiveSideValue>( this._sideValues.Length );
            builder.AddRange( this._sideValues );
            return builder;
        }

        private void Combine( ref ImmutableArray<IReactiveSideValue>.Builder builder, IReactiveSideValue value )
        {
            for ( var i = 0; i < builder.Count; i++ )
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

        /// <summary>
        /// Creates and returns a new <see cref="ReactiveSideValues"/> that combines the values of the current
        /// object with one given other value.
        /// </summary>
        /// <param name="value">The additional value.</param>
        /// <returns>A <see cref="ReactiveSideValues"/> instance that combines the values of the current object
        /// with <paramref name="value"/>.</returns>
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
            if ( this._sideValues.IsDefaultOrEmpty )
            {
                return other;
            }
            else if ( other._sideValues.IsDefaultOrEmpty )
            {
                return this;
            }
            else
            {
                var builder = this.CreateBuilder();
                foreach ( var otherValue in other._sideValues )
                {
                    this.Combine( ref builder, otherValue );
                }

                return new ReactiveSideValues( builder.MoveToImmutable() );
            }
        }
    }
}