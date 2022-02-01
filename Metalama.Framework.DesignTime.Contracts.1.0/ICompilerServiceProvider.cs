// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Entry point exposed by the compiler-side components, to be consumed by the Visual Studio extension.
    /// An instance of Visual Studio can contain several versions of Metalama compiler components, and they
    /// will be represented as different instances of this interface.
    /// </summary>
    public interface ICompilerServiceProvider
    {
        /// <summary>
        /// Gets the version of the compiler-side component.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets a service.
        /// </summary>
        T? GetService<T>()
            where T : class, ICompilerService;

        /// <summary>
        /// Event raised when the compiler-side component is unloaded. (Which now never happens).
        /// </summary>
        event Action Unloaded;
    }
}