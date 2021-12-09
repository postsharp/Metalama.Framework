// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Described semantics of a symbol in intermediate compilation. This allows to differentiate between versions of override targets.
    /// </summary>
    internal enum IntermediateSymbolSemanticKind
    {
        /// <summary>
        /// Default symbol semantic, i.e. version in the intermediate compilation.
        /// </summary>
        Default,

        /// <summary>
        /// Base symbol semantic. Relevant only for introduced override targets with no overridden declaration.
        /// </summary>
        Base,

        /// <summary>
        /// Final version of the symbol. Relevant only for override targets.
        /// </summary>
        Final
    }
}