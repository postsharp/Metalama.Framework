// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Represents a subset of a Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>. The subset is limited
    /// to specific syntax trees.
    /// </summary>
    /// <seealso cref="PartialCompilationExtensions"/>
    public interface IPartialCompilation
    {
        /// <summary>
        /// Gets the Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        Compilation Compilation { get; }

        /// <summary>
        /// Gets the list of syntax trees in the current subset.
        /// </summary>
        ImmutableDictionary<string, SyntaxTree> SyntaxTrees { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="IPartialCompilation"/> is actually partial, or represents a complete compilation.
        /// </summary>
        bool IsPartial { get; }

        /// <summary>
        /// Returns a copy of the current <see cref="IPartialCompilation"/> where the <see cref="SyntaxTrees"/> have been modified.
        /// </summary>
        public IPartialCompilation WithSyntaxTrees(
            IReadOnlyList<ModifiedSyntaxTree>? replacements = null,
            IReadOnlyList<SyntaxTree>? addedTrees = null );

        /// <summary>
        /// Returns a copy of the current <see cref="IPartialCompilation"/> where the <see cref="Resources"/> have been modified.
        /// </summary>
        public IPartialCompilation WithResources( ImmutableArray<ResourceDescription> resources );

        /// <summary>
        /// Gets the list of managed resources for the current compilation. This property is not defined at the design time, only at compile time.
        /// </summary>
        public ImmutableArray<ResourceDescription> Resources { get; }
    }
}