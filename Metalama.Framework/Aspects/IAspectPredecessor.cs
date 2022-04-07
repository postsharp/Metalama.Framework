// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Base interface for objects that can cause aspects to be added to a compilation. Predecessors are exposed on
    /// the <see cref="IAspectInstance.Predecessors"/> property.
    /// </summary>
    [CompileTime]
    [InternalImplement]
    public interface IAspectPredecessor { }
}