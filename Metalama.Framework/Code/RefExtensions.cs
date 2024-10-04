// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for the <see cref="IRef{T}"/> interface.
    /// </summary>
    [PublicAPI]
    [CompileTime]
    public static class RefExtensions
    {
        [Obsolete( "Use the overload that accepts a RefComparison" )]
        public static bool Equals( this IRef a, IRef? b, bool includeNullability )
            => a.Equals( b, includeNullability ? RefComparison.IncludeNullability : RefComparison.Default );

        /// <summary>
        /// Gets the target of the reference for the current execution context, or throws an exception if the reference cannot be resolved.
        /// </summary>
        public static T GetTarget<T>( this IRef<T> reference )
            where T : class, ICompilationElement
            => reference.GetTarget( MetalamaExecutionContext.Current.Compilation );

        /// <summary>
        /// Gets the target of the reference for the current execution context, or returns <c>null</c> if the reference cannot be resolved.
        /// </summary>
        public static T? GetTargetOrNull<T>( this IRef<T> reference )
            where T : class, ICompilationElement
            => reference.GetTargetOrNull( MetalamaExecutionContext.Current.Compilation );
    }
}