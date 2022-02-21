// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Eligibility
{
    /// <summary>
    /// Encapsulates an arbitrary object and its optional human-readable description.
    /// Implemented by <see cref="DescribedObject{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso href="@eligibility"/>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IDescribedObject<out T> : IFormattable
    {
        /// <summary>
        /// Gets the described object.
        /// </summary>
        T Object { get; }

        /// <summary>
        /// Gets the optional human-readable description of <see cref="Object"/>.
        /// </summary>
        FormattableString? Description { get; }
    }
}