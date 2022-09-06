// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Code
{
    // TODO: Verify that it can represent values of run-time-only values and all kinds of arrays.

    /// <summary>
    /// Represents a typed value that can be defined, defined to null, or undefined. Used to represent default values,
    /// for instance <see cref="IParameter.DefaultValue"/>, or custom attribute arguments.
    /// </summary>
    [CompileTime]
    public readonly struct TypedConstant : IExpression
    {
        // ReSharper disable once UnassignedReadonlyField

        private readonly object? _value;
        private readonly IType? _type;

        private void CheckInitialized()
        {
            if ( this._type == null )
            {
                throw new ArgumentNullException( $"The {nameof(TypedConstant)} is unassigned." );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Value"/> has been specified (including when it is set to <c>null</c>).
        /// </summary>
        public bool IsInitialized => this._type != null;

        /// <summary>
        /// Gets the type of the value. This is important if the type is an enum, because in this case, if the enum type is not compile-time,
        /// <see cref="Value"/> is set to the underlying integer value.
        /// </summary>
        public IType Type
        {
            get
            {
                this.CheckInitialized();

                return this._type!;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the value is <c>null</c> or <c>default</c>. Not to be confused with <see cref="IsInitialized"/>.
        /// </summary>
        public bool IsNullOrDefault
        {
            get
            {
                this.CheckInitialized();

                return this.Value == null;
            }
        }

        bool IExpression.IsAssignable => false;

        dynamic? IExpression.Value
        {
            get => SyntaxBuilder.CurrentImplementation.TypedConstant( this );
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For enum values whose type is not a compile-time type, <see cref="Value"/> represents the underlying integer value and <see cref="Type"/> the type of the enum.
        /// </para>
        /// <para>
        /// For enum values whose type is compile-time, <see cref="Value"/> is of enum type.
        /// </para>
        /// <para>
        /// The type <c>ImmutableArray&gt;TypedConstant&gt;</c> is used to represent an array. The <see cref="Values"/> is also set in this case.
        /// </para>
        /// </remarks>
        public object? Value
        {
            get
            {
                this.CheckInitialized();

                return this._value;
            }
        }

        public ImmutableArray<TypedConstant> Values => (this.Value as ImmutableArray<TypedConstant>?).GetValueOrDefault();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedConstant"/> struct that represents the fact that the value
        /// was set to something, even <c>null</c>. To represent the fact that the default value was not set, use <c>default(OptionalValue)</c>.
        /// </summary>
        /// <param name="value">The value (even <c>null</c>).</param>
        internal TypedConstant( IType type, object? value ) : this()
        {
            this._value = value;
            this._type = type;
        }

        public override string ToString() => this._type != null ? this._value?.ToString() ?? "default" : "(uninitialized)";

        public static TypedConstant Default( IType type ) => new( type, null );

        public static TypedConstant Default( Type type ) => new( TypeFactory.GetType( type ), null );

        public static TypedConstant Default<T>() => Default( typeof(T) );

        public static TypedConstant Create( object value ) => new( TypeFactory.GetType( value.GetType() ), value );

        public static TypedConstant Create( IType type, object? value ) => new( type, value );
    }
}