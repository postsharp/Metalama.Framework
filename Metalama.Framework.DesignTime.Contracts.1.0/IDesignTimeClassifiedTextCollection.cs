// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Metalama.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.

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