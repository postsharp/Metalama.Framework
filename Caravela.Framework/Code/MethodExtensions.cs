// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Extension methods for the <see cref="IMethod"/> interface.
    /// </summary>
    public static class MethodExtensions
    {
        /// <summary>
        /// Determines whether a method is a <c>yield</c>-based iterator and returns an <see cref="IteratorInfo"/> value
        /// exposing details about the iterator.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static IteratorInfo GetIteratorInfo( this IMethod method ) => ((ICompilationInternal) method.Compilation).Helpers.GetIteratorInfo( method );
    }
}