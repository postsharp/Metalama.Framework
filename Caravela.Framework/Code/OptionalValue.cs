using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a value that can be defined, defined to null, or undefined. Used to represent default values,
    /// for instance <see cref="IParameter.DefaultValue"/>.
    /// </summary>
    public readonly struct OptionalValue
    {
        private readonly object? _value;

        /// <summary>
        /// Gets a value indicating whether the value has been specified (even set to <c>null</c>).
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public object? Value => this.HasValue ? this._value : throw new InvalidOperationException();

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalValue"/> struct that represents the fact that the value
        /// was set to something, even <c>null</c>. To represent the fact that the default value was not set, use <c>default(OptionalValue)</c>.
        /// </summary>
        /// <param name="value">The value (even <c>null</c>).</param>
        public OptionalValue( object? value ) : this()
        {
            this._value = value;
            this.HasValue = true;
        }
    }
}