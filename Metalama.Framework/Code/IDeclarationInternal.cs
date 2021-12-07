// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Metrics;

namespace Metalama.Framework.Code
{
    internal interface IDeclarationInternal : IDeclaration, IMeasurableInternal
    {
        /// <summary>
        /// Gets the generic declaration, with all unbound generic parameters.
        /// </summary>
        IDeclaration OriginalDefinition { get; }
    }
}