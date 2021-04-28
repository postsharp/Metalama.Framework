// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Represents a subset of a Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>. The subset is limited
    /// to specific syntax trees.
    /// </summary>
    public interface IPartialCompilation
    {
        /// <summary>
        /// Gets the Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        Compilation Compilation { get; }

        /// <summary>
        /// Gets the list of syntax trees in the current subset.
        /// </summary>
        IReadOnlyCollection<SyntaxTree> SyntaxTrees { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="IPartialCompilation"/> is actually partial, or represents a complete compilation.
        /// </summary>
        bool IsPartial { get; }

        /// <summary>
        ///  Adds and replaces syntax trees of the current <see cref="IPartialCompilation"/> and returns a new <see cref="IPartialCompilation"/>
        /// representing the modified object.
        /// </summary>
        public IPartialCompilation UpdateSyntaxTrees( IReadOnlyList<(SyntaxTree OldTree, SyntaxTree NewTree)> replacements, IReadOnlyList<SyntaxTree> addedTrees );
    }
}