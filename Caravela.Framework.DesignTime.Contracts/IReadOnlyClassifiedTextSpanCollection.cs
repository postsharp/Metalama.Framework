// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.

    // The type identifier cannot be modified even during refactoring.
    [Guid( "da58deff-93d5-4d5a-bf6e-11df8bdbd74d" )]
    public interface IReadOnlyClassifiedTextSpanCollection : IReadOnlyCollection<ClassifiedTextSpan>
    {
        TextSpanClassification GetCategory( in TextSpan textSpan );

        IEnumerable<ClassifiedTextSpan> GetClassifiedSpans( TextSpan textSpan );
    }
}