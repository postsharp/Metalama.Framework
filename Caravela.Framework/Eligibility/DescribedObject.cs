// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// Encapsulates an object and a human-readable description.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DescribedObject<T> : IDescribedObject<T>
    {
        /// <inheritdoc />
        public T Object { get; }

        /// <inheritdoc />
        public FormattableString? Description { get; }

        public DescribedObject( T o, FormattableString? description = null )
        {
            this.Object = o;
            this.Description = description;
        }

        string IFormattable.ToString( string? format, IFormatProvider? formatProvider )

            // ReSharper disable FormatStringProblem
            => this.Description?.ToString(CaravelaServices.FormatProvider) ?? string.Format( CaravelaServices.FormatProvider, "{0:" + format + "}", this.Object );
    }
}