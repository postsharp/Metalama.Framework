// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Eligibility
{
    public sealed class DescribedObject<T> : IDescribedObject<T>
    {
        public T Object { get; }

        public FormattableString? Description { get; }

        public IFormatProvider FormatProvider { get; }

        public DescribedObject( T o, IFormatProvider formatProvider, FormattableString? description = null )
        {
            this.Object = o;
            this.FormatProvider = formatProvider;
            this.Description = description;
        }

        public string ToString( string format, IFormatProvider formatProvider )
            => this.Description?.ToString(this.FormatProvider) ?? string.Format( this.FormatProvider, "{0:" + format + "}", this.Object );
    }
}