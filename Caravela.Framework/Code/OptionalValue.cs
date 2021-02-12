using System;

namespace Caravela.Framework.Code
{
    public readonly struct OptionalValue
    {
        private readonly object? _value;

        public bool HasValue { get; }

        public object? Value => this.HasValue ? this._value : throw new InvalidOperationException();

        public OptionalValue( object? value ) : this()
        {
            this._value = value;
            this.HasValue = true;
        }
    }
}