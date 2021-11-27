// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a reference to a declaration that can be resolved using <see cref="GetTarget"/>,
    /// given an compilation, or using the <see cref="RefExtensions.GetTarget{T}"/> extension method
    /// for the compilation of the current context.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the declaration.</typeparam>
    public interface IRef<out T>
        where T : class, ICompilationElement
    {
        string? Serialize();

        /// <summary>
        /// Gets the target of the reference for a given compilation. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTarget{T}"/> extension method.
        /// </summary>
        T GetTarget( ICompilation compilation );
    }
}