// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.AspectWeavers
{
    /// <summary>
    /// Helper methods for implementation of <see cref="IAspectWeaver"/>.
    /// </summary>
    public abstract class AspectWeaverHelper
    {
        internal AspectWeaverHelper() { }

        /// <summary>
        /// Gets an <see cref="ITypeSymbol" /> given a reflection <see cref="Type" />.
        /// </summary>
        public abstract ITypeSymbol? GetTypeSymbol( Type type );
    }
}