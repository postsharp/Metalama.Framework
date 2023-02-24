// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Code.Types;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Metalama.Framework.Code
{
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
                throw new InvalidOperationException( $"The {nameof(TypedConstant)} is unassigned." );
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

        RefKind IHasType.RefKind => RefKind.None;

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

        ref dynamic? IExpression.Value => ref RefHelper.Wrap( SyntaxBuilder.CurrentImplementation.TypedConstant( this ) );

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
        /// <param name="type"></param>
        private TypedConstant( object? value, IType type ) : this()
        {
            if ( value != null )
            {
                var valueType = value.GetType();

                this._value = valueType.IsEnum ? Convert.ChangeType( value, valueType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture ) : value;
            }
            else
            {
                this._value = null;
            }

            this._type = type;
        }

        internal static bool CheckAcceptableType( IType expectedType, object? value, bool throwOnError, IDeclarationFactory declarationFactory )
        {
            if ( value == null )
            {
                // A null value is always acceptable because it means the default value in case of value types.
                return true;
            }

            if ( expectedType is INamedType { FullName: "System.Nullable", TypeArguments: [{ } wrappedType] }
                 && CheckAcceptableType( wrappedType, value, false, declarationFactory ) )
            {
                return true;
            }

            switch (expectedType.SpecialType, value.GetType().Name)
            {
                case (SpecialType.SByte, nameof(SByte)):
                case (SpecialType.Int16, nameof(Int16)):
                case (SpecialType.Int32, nameof(Int32)):
                case (SpecialType.Int64, nameof(Int64)):
                case (SpecialType.Byte, nameof(Byte)):
                case (SpecialType.UInt16, nameof(UInt16)):
                case (SpecialType.UInt32, nameof(UInt32)):
                case (SpecialType.UInt64, nameof(UInt64)):
                case (SpecialType.String, nameof(String)):
                case (SpecialType.Double, nameof(Double)):
                case (SpecialType.Single, nameof(Single)):
                case (SpecialType.Boolean, nameof(Boolean)):
                case (SpecialType.Decimal, nameof(Decimal)):
                case (SpecialType.Object, nameof(SByte)):
                case (SpecialType.Object, nameof(Int16)):
                case (SpecialType.Object, nameof(Int32)):
                case (SpecialType.Object, nameof(Int64)):
                case (SpecialType.Object, nameof(Byte)):
                case (SpecialType.Object, nameof(UInt16)):
                case (SpecialType.Object, nameof(UInt32)):
                case (SpecialType.Object, nameof(UInt64)):
                case (SpecialType.Object, nameof(String)):
                case (SpecialType.Object, nameof(Double)):
                case (SpecialType.Object, nameof(Single)):
                    return true;
            }

            if ( expectedType is INamedType { FullName: "System.Type" } )
            {
                if ( value is not IType )
                {
                    if ( throwOnError )
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            $"The value should be of type 'IType' but is of type '{value.GetType()}'." );
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if ( expectedType is IArrayType arrayType )
            {
                if ( value is Array array )
                {
                    if ( !arrayType.ElementType.Equals( declarationFactory.GetTypeByReflectionType( array.GetType().GetElementType()! ) ) )
                    {
                        if ( throwOnError )
                        {
                            throw new ArgumentOutOfRangeException(
                                nameof(value),
                                $"The value should be of type '{expectedType}' but is of type '{value.GetType()}'." );
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else if ( value is not ImmutableArray<TypedConstant> immutableArray )
                {
                    if ( throwOnError )
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            $"The value should be of type '{typeof(ImmutableArray<TypedConstant>)}' but is of type '{value.GetType()}'." );
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    foreach ( var arrayItem in immutableArray )
                    {
                        if ( !CheckAcceptableType( arrayType.ElementType, arrayItem._value, throwOnError, declarationFactory ) )
                        {
                            return false;
                        }
                    }
                }
            }
            else if ( expectedType.TypeKind == TypeKind.Enum )
            {
                if ( !expectedType.Equals( declarationFactory.GetTypeByReflectionType( value.GetType() ) ) )
                {
                    if ( !CheckAcceptableType( ((INamedType) expectedType).UnderlyingType, value, throwOnError, declarationFactory ) )
                    {
                        return false;
                    }
                }
            }
            else if ( expectedType is INamedType { FullName: "System.Type" } )
            {
                if ( value is not (IType or System.Type) )
                {
                    if ( throwOnError )
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            $"The value should be of type '{typeof(IType)}' or '{typeof(Type)}' but is of type '{value.GetType()}'." );
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if ( throwOnError )
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"The value should be of type '{expectedType}' but is of type '{value.GetType()}'." );
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() => this._type != null ? this._value?.ToString() ?? "default" : "(uninitialized)";

        private static IType GetIType( Type type ) => TypeFactory.GetType( FixRuntimeType( type ) );

        private static Type FixRuntimeType( Type type ) => type is not ICompileTimeType && typeof(Type).IsAssignableFrom( type ) ? typeof(Type) : type;

        private static object? FixValue( object? value ) => value is Type type ? TypeFactory.GetType( type ) : value;

        public static TypedConstant Default( IType type ) => new( null, type );

        public static TypedConstant Default( Type type ) => new( null, GetIType( type ) );

        public static TypedConstant Create( object value ) => Create( value, value.GetType() );

        public static TypedConstant Create( object? value, Type type ) => Create( value, GetIType( type ) );

        public static TypedConstant Create( object? value, IType type )
        {
            var fixedValue = FixValue( value );

            CheckAcceptableType( type, fixedValue, true, ((ICompilationInternal) type.Compilation).Factory );

            return new TypedConstant( fixedValue, type );
        }

        public static TypedConstant CreateUnchecked( object? value, IType type ) => new( value, type );

        internal static TypedConstant UnwrapOrCreate( object? value, IType type )
            => value is TypedConstant typedConstant ? typedConstant : new TypedConstant( value, type );

        public TypedConstant ForCompilation( ICompilation compilation )
        {
            if ( !this.IsInitialized )
            {
                return default;
            }
            else if ( this.Type.Compilation == compilation )
            {
                return this;
            }
            else
            {
                return this.Value switch
                {
                    IType type => Create( type.ForCompilation( compilation ), this.Type.ForCompilation( compilation ) ),
                    ImmutableArray<TypedConstant> array => Create(
                        array.Select( i => i.ForCompilation( compilation ) ),
                        this.Type.ForCompilation( compilation ) ),
                    _ => Create( this.Value, this.Type.ForCompilation( compilation ) )
                };
            }
        }
    }
}