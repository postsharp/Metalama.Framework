// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Described semantics of a symbol in intermediate compilation. This allows to differentiate between versions of override targets.
    /// </summary>
    internal enum IntermediateSymbolSemanticKind
    {
        /// <summary>
        /// Default symbol semantic, i.e. version visible in the intermediate compilation.
        /// For aspect-overridden members, this is the "_Source" member, i.e. the original body.
        /// </summary>
        Default,

        /// <summary>
        /// Base symbol semantic. Represents the previous version of the symbol before the current type.
        /// </summary>
        Base,

        /// <summary>
        /// Final version of the symbol. Relevant only for override targets.
        /// </summary>
        Final
    }
}