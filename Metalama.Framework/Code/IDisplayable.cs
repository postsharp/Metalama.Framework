// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Defines a method <see cref="ToDisplayString"/> that renders the current declaration into a human-readable
    /// string.
    /// </summary>
    [CompileTime]
    [InternalImplement]
    public interface IDisplayable
    {
        /// <summary>
        /// Renders the current declaration into a human-readable string.
        /// </summary>
        /// <param name="format">Reserved for future use. Specifies formatting options.</param>
        /// <param name="context">Reserved for future use. Specifies the context in which the string must be displayed. This allow to abbreviate a few pieces of information.</param>
        /// <returns>A human-readable string for the current declaration.</returns>
        string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
    }
}