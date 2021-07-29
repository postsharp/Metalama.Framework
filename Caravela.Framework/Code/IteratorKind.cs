// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable InconsistentNaming

namespace Caravela.Framework.Code
{
    public enum IteratorKind
    {
        None,
        IEnumerable,
        IEnumerator,
        UntypedIEnumerable,
        UntypedIEnumerator,
        IAsyncEnumerable,
        IAsyncEnumerator
    }
}