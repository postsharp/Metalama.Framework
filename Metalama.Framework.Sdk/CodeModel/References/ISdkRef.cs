// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// Represents a reference to a declaration that can be resolved using <see cref="IRef{T}.GetTarget"/>.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the declaration.</typeparam>
    internal interface ISdkRef<out T> : IRef<T>
        where T : class, ICompilationElement
    {
        // This is a temporary method to extract the symbol from the reference, when there is any.
        // In the final implementation, this method should not be necessary.
        ISymbol? GetSymbol( Compilation compilation );
    }
}