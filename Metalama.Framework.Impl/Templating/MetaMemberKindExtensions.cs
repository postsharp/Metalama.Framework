// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Templating
{
    internal static class MetaMemberKindExtensions
    {
        public static bool IsAnyProceed( this MetaMemberKind kind )
            => kind switch
            {
                MetaMemberKind.Proceed => true,
                MetaMemberKind.ProceedAsync => true,
                MetaMemberKind.ProceedEnumerable => true,
                MetaMemberKind.ProceedEnumerator => true,
                MetaMemberKind.ProceedAsyncEnumerable => true,
                MetaMemberKind.ProceedAsyncEnumerator => true,
                _ => false
            };
    }
}