// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Maps an old <see cref="SyntaxTree"/> to a new <see cref="SyntaxTree"/> in an <see cref="IPartialCompilation"/>.
    /// </summary>
    public readonly struct ModifiedSyntaxTree
    {
        public string FilePath => this.NewTree.FilePath;

        /// <summary>
        /// Gets the old syntax tree, or <c>null</c> if this is a new tree.
        /// </summary>
        public SyntaxTree? OldTree { get; }

        /// <summary>
        /// Gets the new syntax tree.
        /// </summary>
        public SyntaxTree NewTree { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiedSyntaxTree"/> struct.
        /// </summary>
        public ModifiedSyntaxTree( SyntaxTree newTree, SyntaxTree? oldTree = null )
        {
            if ( oldTree != null && !string.Equals( oldTree.FilePath, newTree.FilePath, StringComparison.Ordinal ) )
            {
                throw new ArgumentOutOfRangeException( nameof(newTree), "The FilePath property of both trees must be equal." );
            }

            this.OldTree = oldTree;
            this.NewTree = newTree;
        }
    }
}