// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Encapsulates dotted strings such as namespaces and type names, so their
    /// serialization by <see cref="PortableFormatter"/> can be optimized.
    /// </summary>
    internal struct DottedString : IEquatable<DottedString>
    {
        private readonly string value;

        /// <summary>
        /// Initializes a new <see cref="DottedString"/>.
        /// </summary>
        /// <param name="value">Value.</param>
        public DottedString( string value )
        {
            this.value = value;
        }

        /// <summary>
        /// Gets an instance of <see cref="DottedString"/> representing a <c>null</c> string.
        /// </summary>
        public static readonly DottedString Null = new DottedString( Null );

        /// <summary>
        /// Determines whether the current <see cref="DottedString"/> represents a <c>null</c> string.
        /// </summary>
        public bool IsNull
        {
            get { return this.value == null; }
        }

        /// <summary>
        /// Gets the string encapsulated by the current <see cref="DottedString"/>.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Converts a <see cref="DottedString"/> into a <see cref="String"/>.
        /// </summary>
        /// <param name="dottedString">A <see cref="DottedString"/>.</param>
        /// <returns>The <see cref="String"/> encapsulated by <paramref name="dottedString"/>.</returns>
        public static implicit operator string( DottedString dottedString )
        {
            return dottedString.ToString();
        }

#pragma warning disable CA2225 // Operator overloads have named alternates (there is a constructor)
        /// <summary>
        /// Converts a <see cref="String"/> into a <see cref="DottedString"/>.
        /// </summary>
        /// <param name="str">A <see cref="String"/>.</param>
        /// <returns>A <see cref="DottedString"/> encapsulating <paramref name="str"/>.</returns>
        public static implicit operator DottedString( string str)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return new DottedString( str );
        }

        /// <summary>
        /// Determines whether the current<see cref="DottedString"/> is equal to another one.
        /// </summary>
        /// <param name="other">Another <see cref="DottedString"/>.</param>
        /// <returns><c>true</c> if the current <see cref="DottedString"/> equals <c>other</c>, otherwise <c>false</c>.</returns>
        public bool Equals( DottedString other )
        {
            return string.Equals( this.value, other.value, StringComparison.Ordinal );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.value == null ? 0 : this.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if (obj is DottedString other)
                return this.Equals(other);
            else
                return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.value;
        }

        /// <summary>
        /// Operator.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(DottedString left, DottedString right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Operator.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(DottedString left, DottedString right)
        {
            return !(left == right);
        }
    }
}