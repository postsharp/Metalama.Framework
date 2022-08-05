// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal record CompilationVersion(
    Compilation Compilation,
    ulong CompileTimeProjectHash,
    ImmutableDictionary<string, SyntaxTreeVersion> SyntaxTrees ) : ICompilationVersion
{
    AssemblyIdentity ICompilationVersion.AssemblyIdentity => this.Compilation.Assembly.Identity;

    public bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion ) 
        => this.SyntaxTrees.TryGetValue( path, out  syntaxTreeVersion );
    
}