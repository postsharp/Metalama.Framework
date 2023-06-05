// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal readonly record struct SyntaxTreeVersion(
    SyntaxTree SyntaxTree,
    bool HasCompileTimeCode = false,
    ulong DeclarationHash = 0,
    ImmutableArray<TypeDependencyKey> PartialTypes = default,
    int PartialTypesHash = 0 )
{
    private readonly SyntaxTree? _syntaxTree = SyntaxTree;

    public SyntaxTree SyntaxTree => this._syntaxTree ?? throw new ArgumentNullException();

    public bool IsDefault => this._syntaxTree == null;

    public SyntaxTreeVersion( SyntaxTree syntaxTree, in SyntaxTreeVersionData data ) : this(
        syntaxTree,
        data.HasCompileTimeCode,
        data.DeclarationHash,
        data.PartialTypes,
        data.PartialTypesHash ) { }
}

internal readonly record struct SyntaxTreeVersionData(
    bool HasCompileTimeCode = false,
    ulong DeclarationHash = 0,
    ImmutableArray<TypeDependencyKey> PartialTypes = default,
    int PartialTypesHash = 0 )
{
    public SyntaxTreeVersionData( in SyntaxTreeVersion version ) : this(
        version.HasCompileTimeCode,
        version.DeclarationHash,
        version.PartialTypes,
        version.PartialTypesHash ) { }
}