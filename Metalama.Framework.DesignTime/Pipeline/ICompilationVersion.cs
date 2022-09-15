// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal interface ICompilationVersion
{
    AssemblyIdentity AssemblyIdentity { get; }

    bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion );

    Compilation Compilation { get; }

    /// <summary>
    /// Gets the compilations directly referenced by the current <see cref="ICompilationVersion"/>.
    /// </summary>
    ImmutableDictionary<AssemblyIdentity, ICompilationVersion> ReferencedCompilations { get; }
}