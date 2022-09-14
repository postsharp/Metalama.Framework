// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal interface ICompilationVersion
{
    AssemblyIdentity AssemblyIdentity { get; }

    ulong CompileTimeProjectHash { get; }

    bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion );

    Compilation Compilation { get; }

    IEnumerable<string> EnumerateSyntaxTreePaths();

    ImmutableDictionary<AssemblyIdentity, ICompilationVersion> References { get; }
}