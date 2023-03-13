// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Project;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for the <see cref="IRef{T}"/> interface.
    /// </summary>
    [PublicAPI]
    [CompileTime]
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