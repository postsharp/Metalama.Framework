// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Sdk
{
    /// <summary>
    /// Helper methods for implementation of <see cref="IAspectWeaver"/>.
    /// </summary>
    [InternalImplement]
    public interface IAspectWeaverHelper
    {
        /// <summary>
        /// Gets an <see cref="ITypeSymbol"/> given a reflection <see cref="Type"/>.
        /// </summary>
        ITypeSymbol? GetTypeSymbol( Type type );
    }
}