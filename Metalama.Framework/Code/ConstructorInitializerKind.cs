// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Describes the kind of constructor initializer.
    /// </summary>
    [CompileTime]
    public enum ConstructorInitializerKind
    {
        /// <summary>
        /// The constructor has no explicit initializer.
        /// </summary>
        None,

        /// <summary>
        /// The initializer refers to the base constructor.
        /// </summary>
        Base,

        /// <summary>
        /// The initializer reference another constructor of the same type.
        /// </summary>
        This
    }
}