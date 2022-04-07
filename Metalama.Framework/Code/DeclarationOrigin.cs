// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Origins of a declaration.
    /// </summary>
    [CompileTime]
    public enum DeclarationOrigin
    {
        /// <summary>
        /// Source code.
        /// </summary>
        Source,

        /// <summary>
        /// Roslyn code generator.
        /// </summary>
        Generator,

        /// <summary>
        /// Aspect (introduction).
        /// </summary>
        Aspect
    }
}