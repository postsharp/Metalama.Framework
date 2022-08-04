// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal class CompilationDependency
{
    private static readonly ImmutableDictionary<string, SyntaxTreeDependencyCollection> _emptySyntaxTreeDependencies =
        ImmutableDictionary<string, SyntaxTreeDependencyCollection>.Empty.WithComparers( StringComparer.Ordinal );

    private static readonly ImmutableDictionary<string, ulong> _emptyCompileTimeProjectDependencies =
        ImmutableDictionary<string, ulong>.Empty.WithComparers( StringComparer.Ordinal );

    public AssemblyIdentity AssemblyIdentity { get; }

    public ulong CompileTimeProjectHash { get; }

    public ImmutableDictionary<string, SyntaxTreeDependencyCollection> SyntaxTreeDependencies { get; } = _emptySyntaxTreeDependencies;

    public ImmutableDictionary<string, ulong> CompileTimeProjectDependencies { get; } = _emptyCompileTimeProjectDependencies;

    public CompilationDependency( AssemblyIdentity assemblyIdentity )
    {
        this.AssemblyIdentity = assemblyIdentity;
    }

    public CompilationDependency Update( CompilationPipelineResult pipelineResult, IReadOnlyCollection<DependencyEdge> dependencies )
    {
        throw new NotImplementedException();
    }
}