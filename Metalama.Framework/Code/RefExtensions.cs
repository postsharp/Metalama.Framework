// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for the <see cref="IRef{T}"/> interface.
    /// </summary>
    public static class RefExtensions
    {
        /// <summary>
        /// Gets the target of the reference for the current execution context.
        /// </summary>
        public static T GetTarget<T>( this IRef<T> reference, ReferenceResolutionOptions options )
            where T : class, ICompilationElement
            => reference.GetTarget( MetalamaExecutionContext.Current.Compilation, options );
    }
}