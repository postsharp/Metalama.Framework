namespace Caravela.Reactive
{
    /// <summary>
    /// Represents a versioned value. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ReactiveVersionedValue<T> : IReactiveVersionedValue<T>
    {

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