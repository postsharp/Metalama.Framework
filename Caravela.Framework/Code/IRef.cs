// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a reference to a declaration that can be resolved using <see cref="GetTarget"/>,
    /// given an compilation.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the declaration.</typeparam>
    public interface IRef<out T>
        where T : class, ICompilationElement
    {
        T GetTarget( ICompilation compilation );
    }

    public static class RefExtensions
    {
        public static T GetTarget<T>( this IRef<T> reference )
            where T : class, ICompilationElement
            => reference.GetTarget( CaravelaExecutionContext.Current.Compilation ?? throw new InvalidOperationException() );
    }
}