// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        /// Explicitly or implicitly declared in the source code.
        /// </summary>
        Source,

        /// <summary>
        /// Synthesized by the code model (e.g. "getter" for a field).
        /// </summary>
        PseudoSource,

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