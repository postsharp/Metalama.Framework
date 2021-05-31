// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using System.Collections.Immutable;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectClassBuilder
    {
        string DisplayName { get; set; }

        string? Description { get; set; }

        ImmutableArray<string> Layers { get; set; }

        IAspectDependencyBuilder Dependencies { get; }
    }
}