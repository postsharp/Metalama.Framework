// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Scope of introduction advices.
    /// </summary>
    [CompileTimeOnly]
    public enum IntroductionScope
    {
        /// <summary>
        /// If the advice template is static, the behavior is the same as <see cref="Static"/>, otherwise behavior is the same as <see cref="Target"/>.
        /// </summary>
        Default,

        /// <summary>
        /// Introduced member will be always of instance scope.
        /// </summary>
        Instance,

        /// <summary>
        /// Introduced member will be always of static scope.
        /// </summary>
        Static,

        /// <summary>
        /// Introduced member will be always of the same scope as the target declaration.
        /// </summary>
        Target
    }
}