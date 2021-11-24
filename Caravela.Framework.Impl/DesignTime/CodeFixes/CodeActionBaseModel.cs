// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a code action or a code action menu.
    /// </summary>
    internal abstract class CodeActionBaseModel
    {
        protected const string TitleJoin = ": ";

        /// <summary>
        /// Gets the code action title.
        /// </summary>
        public string Title { get; }

        protected CodeActionBaseModel( string title )
        {
            this.Title = title;
        }

        /// <summary>
        /// Gets the single code action, or the collection of code actions, that is represented by the current element.
        /// </summary>
        /// <param name="supportsHierarchicalItems">Indicates whether the current IDE supports hierarchical items.</param>
        /// <param name="titlePrefix">A string to be prepended to <see cref="Title"/>.</param>
        /// <returns></returns>
        public abstract ImmutableArray<CodeAction> ToCodeActions( bool supportsHierarchicalItems, string titlePrefix = "" );
    }
}