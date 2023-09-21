// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        /// Gets a <see cref="ConstructorInfo"/> that represents the current constructor at run time.
        /// </summary>
        /// <returns>A <see cref="ConstructorInfo"/> that can be used only in run-time code.</returns>
        ConstructorInfo ToConstructorInfo();
        
        /// <summary>
        /// Gets the definition of the constructor. If the current declaration is a constructor of
        /// a generic type instance, this returns the constructor in the generic type definition. Otherwise, it returns the current instance.
        /// </summary>
        new IConstructor Definition { get; }
    }
}