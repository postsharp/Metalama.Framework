namespace Caravela.Reactive
{
    public readonly struct ReactiveVersionedValue<T>
    {
        public ReactiveVersionedValue(T value, int version)
        {
            Version = version;
            Value = value;
        }

        public int Version { get; }
        public T Value { get; }
    }
}