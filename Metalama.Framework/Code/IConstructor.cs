// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents an instance constructor or a static constructor.
    /// </summary>
    public interface IConstructor : IMethodBase
    {
        /// <summary>
        /// Gets a <see cref="ConstructorInitializerKind" /> that specifies the initializer semantics of the constructor.
        /// </summary>
        public ConstructorInitializerKind InitializerKind { get; }

        /// <summary>
        /// Gets a value indicating whether the constructor is explicitly defined. Returns <c>false</c> this information cannot be determined.
        /// </summary>
        public bool IsExplicit { get; }

        /// <summary>
        /// Gets a <see cref="ConstructorInfo"/> that represents the current constructor at run time.
        /// </summary>
        /// <returns>A <see cref="ConstructorInfo"/> that can be used only in run-time code.</returns>
        ConstructorInfo ToConstructorInfo();
    }
}