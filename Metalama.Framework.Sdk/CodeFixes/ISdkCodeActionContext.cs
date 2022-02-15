// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeFixes;

/// <summary>
/// Extends the <see cref="ICodeActionContext"/> interface with members that can be used
/// by custom implementations of code fixes using the SDK.
/// </summary>
public interface ISdkCodeActionContext : ICodeActionContext
{
    /// <summary>
    /// Gets the current compilation. It must be updated using <see cref="UpdateTree(Microsoft.CodeAnalysis.SyntaxTree,Microsoft.CodeAnalysis.SyntaxTree)"/>
    /// or <see cref="ApplyModifications"/>.
    /// </summary>
    IPartialCompilation Compilation { get; }

    /// <summary>
    /// Updates a <see cref="SyntaxTree"/>.
    /// </summary>
    void UpdateTree( SyntaxTree transformedTree, SyntaxTree originalTree );

    /// <summary>
    /// Updates a <see cref="SyntaxTree"/> by passing the new root syntax node.
    /// </summary>
    void UpdateTree( SyntaxNode transformedRoot, SyntaxTree originalTree );

    /// <summary>
    /// Applies the modifications accumulated in a partial compilation, i.e. those done by <see cref="IPartialCompilation.WithSyntaxTreeModifications"/>.
    /// </summary>
    void ApplyModifications( IPartialCompilation compilation );
}