// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Validation;
using System;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// Encapsulates an arbitrary object and its optional human-readable description, as well as an <see cref="IFormatProvider"/>.
    /// (Not implemented.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InternalImplement]
    [Obsolete( "Not implemented." )]
    [CompileTimeOnly]
    public interface IDescribedObject<out T> : IFormattable
    {
        T Object { get; }

        FormattableString? Description { get; }

        // The reason to include an IFormatProvider here is opportunistic: some implementations of IEligibilityRule need
        // to format substrings and require our custom formatter, so it is easy to pass it as a part of this object.

        IFormatProvider FormatProvider { get; }
    }
}