namespace Caravela.Reactive
{
    public readonly struct ReactiveVersionedValue<T> : IReactiveVersionedValue<T>
    {
        public ReactiveVersionedValue(T value, int version)
        {
            this.Version = version;
            this.Value = value;
        }

        public int Version { get; }
        public T Value { get; }
    }
}