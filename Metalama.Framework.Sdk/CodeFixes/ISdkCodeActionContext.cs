// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeFixes;

/// <summary>
/// Extends the <see cref="ICodeActionContext"/> interface with members that can be used
/// by custom implementations of code fixes using the SDK.
/// </summary>
[PublicAPI]
public interface ISdkCodeActionContext : ICodeActionContext
{
    /// <summary>
    /// Gets the current compilation. It must be updated using <see cref="UpdateTree(Microsoft.CodeAnalysis.SyntaxTree,Microsoft.CodeAnalysis.SyntaxTree)"/>
    /// or <see cref="UpdateCompilation"/>.
    /// </summary>
    IPartialCompilation Compilation { get; }

    /// <summary>
    /// Updates a <see cref="SyntaxTree" />.
    /// </summary>
    void UpdateTree( SyntaxTree transformedTree, SyntaxTree originalTree );

    /// <summary>
    /// Updates a <see cref="SyntaxTree" /> by passing the new root syntax node.
    /// </summary>
    void UpdateTree( SyntaxNode transformedRoot, SyntaxTree originalTree );

    /// <summary>
    /// Applies the modifications accumulated in a partial compilation, i.e. those done by <see cref="IPartialCompilation.WithSyntaxTreeTransformations"/>.
    /// </summary>
    void UpdateCompilation( IPartialCompilation compilation );
}