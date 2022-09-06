// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.AspectWeavers
{
    /// <summary>
    /// Helper methods for implementation of <see cref="IAspectWeaver"/>.
    /// </summary>
    [InternalImplement]
    public interface IAspectWeaverHelper
    {
        /// <summary>
        /// Gets an <see cref="ITypeSymbol" /> given a reflection <see cref="Type"/>.
        /// </summary>
        ITypeSymbol? GetTypeSymbol( Type type );
    }
}