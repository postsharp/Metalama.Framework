// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal class CompilationDependencyCollection
{
    public static CompilationDependencyCollection Empty { get; } = new();

    public ImmutableDictionary<AssemblyIdentity, CompilationDependency> Compilations { get; } =
        ImmutableDictionary<AssemblyIdentity, CompilationDependency>.Empty;

    public CompilationDependencyCollection Update( AspectPipelineResult? pipelineResult, IReadOnlyCollection<DependencyEdge> dependencies )
    {
        throw new NotImplementedException();
    }

    private CompilationDependencyCollection() { }
}