// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal class CompilationDependency
{
    public ProjectId ProjectId { get; }

    public ulong Hash { get; }

    public ImmutableDictionary<string, SyntaxTreeDependency> SyntaxTreeDependencies { get; }
    
    public ImmutableDictionary<string, ulong> CompileTimeProjectDependencies { get; }
}