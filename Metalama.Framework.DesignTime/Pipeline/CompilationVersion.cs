// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal record class CompilationVersion(
    Compilation Compilation,
    ulong CompileTimeProjectHash,
    ImmutableDictionary<string, SyntaxTreeVersion> SyntaxTrees ) : ICompilationVersion
{
    AssemblyIdentity ICompilationVersion.AssemblyIdentity => this.Compilation.Assembly.Identity;

    public bool TryGetSyntaxTreeDeclarationHash( string path, out ulong hash )
    {
        if ( this.SyntaxTrees.TryGetValue( path, out var syntaxTreeVersion ) )
        {
            hash = syntaxTreeVersion.DeclarationHash;

            return true;
        }
        else
        {
            hash = 0;

            return false;
        }
    }

    public bool TryGetSyntaxTreePartialTypesHash( string path, out ulong hash )
    {
        if ( this.SyntaxTrees.TryGetValue( path, out var syntaxTreeVersion ) )
        {
            hash = syntaxTreeVersion.PartialTypesHash;

            return true;
        }
        else
        {
            hash = 0;

            return false;
        }
    }
}