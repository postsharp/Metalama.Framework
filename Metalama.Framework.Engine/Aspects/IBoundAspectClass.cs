// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// An <see cref="IAspectClass"/> for which an <see cref="IAspectDriver"/> has been created.
    /// </summary>
    internal interface IBoundAspectClass : IAspectClassImpl
    {
        IAspectDriver AspectDriver { get; }

        Location? DiagnosticLocation { get; }
    }
}