// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a typed value that can be defined, defined to null, or undefined. Used to represent default values,
    /// for instance <see cref="IParameter.DefaultValue"/>, or custom attribute arguments. For enum values whose type is not a compile-time
    /// type, <see cref="Value"/> represents the underlying integer value and <see cref="Type"/> the type of the enum. For enum values whose
    /// type is compile-time, <see cref="Value"/> is of enum type.
    /// </summary>
    public readonly struct TypedConstant
    {
        private readonly object? _value;
        private readonly IType? _type;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Value"/> has been specified (including when it is set to <c>null</c>).
        /// </summary>
        public bool IsAssigned => this._type != null;

        /// <summary>
        /// Gets the type of the value. This is important if the type is an enum, because in this case, if the enum type is not compile-time,
        /// <see cref="Value"/> is set to the underlying integer value.
        /// </summary>
        public IType Type => this._type ?? throw new ArgumentNullException($"The {nameof(TypedConstant)} is unassigned.");

        /// <summary>
        /// Gets a value indicating whether the value is <c>null</c>. Not to be confused with <see cref="IsAssigned"/>.
        /// </summary>
        public bool IsNull => this.Value == null;

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public object? Value => this.IsAssigned ? this._value : throw new InvalidOperationException();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedConstant"/> struct that represents the fact that the value
        /// was set to something, even <c>null</c>. To represent the fact that the default value was not set, use <c>default(OptionalValue)</c>.
        /// </summary>
        /// <param name="value">The value (even <c>null</c>).</param>
        public TypedConstant( IType type, object? value ) : this()
        {
            this._value = value;
            this._type = type;
        }
    }
}