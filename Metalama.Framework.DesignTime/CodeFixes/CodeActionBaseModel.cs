// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a code action or a code action menu.
    /// </summary>
    [JsonObject]
    public abstract class CodeActionBaseModel : ICodeActionModel
    {
        protected const string TitleJoin = ": ";

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
        /// Gets the single <see cref="CodeAction"/>, or the collection of code actions, that is represented by the current element.
        /// This method is invoked in the user process.
        /// </summary>
        /// <param name="invocationContext">The invocation context.</param>
        /// <param name="titlePrefix">A string to be prepended to <see cref="Title"/>.</param>
        public abstract ImmutableArray<CodeAction> ToCodeActions( CodeActionInvocationContext invocationContext, string titlePrefix = "" );
    }
}