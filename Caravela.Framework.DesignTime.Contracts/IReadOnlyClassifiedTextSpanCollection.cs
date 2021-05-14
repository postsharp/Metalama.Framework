// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Caravela.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.

    /// <summary>
    /// A read-only collection of <see cref="ClassifiedTextSpan"/> with additional methods.
    /// </summary>
    [Guid( "da58deff-93d5-4d5a-bf6e-11df8bdbd74d" )]
    public interface IReadOnlyClassifiedTextSpanCollection : IReadOnlyCollection<ClassifiedTextSpan>
    {
        /// <summary>
        /// Gets the classification of a given <see cref="TextSpan"/>.
        /// </summary>
        TextSpanClassification GetCategory( in TextSpan textSpan );

        /// <summary>
        /// Gets all <see cref="ClassifiedTextSpan"/> in a given <see cref="TextSpan"/>. 
        /// </summary>
        IEnumerable<ClassifiedTextSpan> GetClassifiedSpans( TextSpan textSpan );
    }
}