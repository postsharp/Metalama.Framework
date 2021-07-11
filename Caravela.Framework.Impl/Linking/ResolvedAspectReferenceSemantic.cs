// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Linking
{
    internal enum ResolvedAspectReferenceSemantic
    {
        /// <summary>
        /// Default symbol semantic, i.e. what this symbol would be in the final compilation.
        /// </summary>
        Default,

        /// <summary>
        /// Original version of the symbol. Relevant only for override targets.
        /// </summary>
        Original,
    }
}
