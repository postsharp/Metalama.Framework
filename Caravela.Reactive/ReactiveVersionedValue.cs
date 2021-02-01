namespace Caravela.Reactive
{
    /// <summary>
    /// Represents a versioned reactive value: a <see cref="Value"/>, <see cref="SideValues"/> and a <see cref="Version"/>
    /// number. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ReactiveVersionedValue<T> : IReactiveVersionedValue<T>
    {

        /// <summary>
        /// Initializes a new <see cref="ReactiveVersionedValue{T}"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="version">The version number.</param>
        /// <param name="sideValues">The side values, if any.</param>
        public ReactiveVersionedValue( T value, int version, ReactiveSideValues sideValues = default )
        {
            this.Version = version;
            this.Value = value;
            this.SideValues = sideValues;
        }

        public int Version { get; }
        public T Value { get; }
        public ReactiveSideValues SideValues { get; }

    }
}