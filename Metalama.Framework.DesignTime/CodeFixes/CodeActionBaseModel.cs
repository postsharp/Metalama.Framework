// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a code action or a code action menu.
    /// </summary>
    public abstract class CodeActionBaseModel : ICodeAction
    {
        protected const string TitleJoin = ": ";
        protected const int NextKey = 1;

        /// <summary>
        /// Gets or sets the code action title.
        /// </summary>
        public string Title { get; set; }

        protected CodeActionBaseModel( string title )
        {
            this.Title = title;
        }

        // Deserialization constructor.
        protected CodeActionBaseModel()
        {
            this.Title = null!;
        }

        /// <summary>
        /// Gets the single code action, or the collection of code actions, that is represented by the current element.
        /// </summary>
        /// <param name="invocationContext"></param>
        /// <param name="titlePrefix">A string to be prepended to <see cref="Title"/>.</param>
        /// <returns></returns>
        public abstract ImmutableArray<CodeAction> ToCodeActions( CodeActionInvocationContext invocationContext, string titlePrefix = "" );
    }
}