namespace Caravela.Framework.Code
{
    public readonly struct OptionalValue
    {
        public bool HasValue { get; }

        public object? Value { get; }

        public OptionalValue( object? value ) : this()
        {
            this.Value = value;
            this.HasValue = true;
        }
    }
}