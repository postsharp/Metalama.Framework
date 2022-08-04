// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public interface IDependencyCollector : IService
{
    void AddDependency( INamedTypeSymbol masterSymbol, INamedTypeSymbol dependentSymbol );
}