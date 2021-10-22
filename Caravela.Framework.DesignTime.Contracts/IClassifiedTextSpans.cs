// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Caravela.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.

    /// <summary>
    /// A read-only collection of <see cref="IClassifiedTextSpan"/> with additional methods.
    /// </summary>
    [Guid( "da58deff-93d5-4d5a-bf6e-11df8bdbd74d" )]
    [ComImport]
    public interface IClassifiedTextSpans
    {
        IEnumerable<IClassifiedTextSpan> GetClassifiedTextSpans();
        
        /// <summary>
        /// Gets all <see cref="IClassifiedTextSpan"/> in a given <see cref="TextSpan"/>. 
        /// </summary>
        IEnumerable<IClassifiedTextSpan> GetClassifiedTextSpans( TextSpan textSpan );
    }
}