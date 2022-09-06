// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// A read-only collection of <see cref="DesignTimeClassifiedTextSpan"/> with additional methods.
    /// </summary>
    public interface IDesignTimeClassifiedTextCollection
    {
        IEnumerable<DesignTimeClassifiedTextSpan> GetClassifiedTextSpans();

        /// <summary>
        /// Gets all <see cref="DesignTimeClassifiedTextSpan"/> in a given <see cref="TextSpan"/>. 
        /// </summary>
        IEnumerable<DesignTimeClassifiedTextSpan> GetClassifiedTextSpans( TextSpan textSpan );
    }
}