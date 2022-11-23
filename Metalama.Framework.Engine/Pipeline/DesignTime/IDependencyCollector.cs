// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public interface IDependencyCollector : IProjectService
{
    void AddDependency( INamedTypeSymbol masterSymbol, INamedTypeSymbol dependentSymbol );
}