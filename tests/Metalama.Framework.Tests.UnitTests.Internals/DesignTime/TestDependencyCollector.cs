// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline.DesignTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class TestDependencyCollector : IDependencyCollector
{
    public HashSet<string> Dependencies { get; } = new();

    public void AddDependency( INamedTypeSymbol masterSymbol, INamedTypeSymbol dependentSymbol )
    {
        this.Dependencies.Add( $"{dependentSymbol}->{masterSymbol}" );
    }
}