// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading.Tasks;

namespace Metalama.Framework.CodeFixes
{
    /// <summary>
    /// Represents a modification of the current solution, including the <see cref="Title"/> of transformation.
    /// To instantiate a single-step code fix, use <see cref="CodeFixFactory"/>. To instantiate a more complex code fix, use the constructor.
    /// </summary>
    public sealed class CodeFix
    {
        /// <summary>
        /// Gets the title of the <see cref="CodeFix"/>, displayed to the user in the light bulb or refactoring menu.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the action that transforms the solution.
        /// </summary>
        internal Func<ICodeActionBuilder, Task> CodeAction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFix"/> class. This constructor must only be used to create multi-transformations
        /// code fixes. For single-step code fixes, use <see cref="CodeFixFactory"/>.
        /// </summary>
        /// <param name="title">Title of the code fix, shown to the user (must be unique).</param>
        /// <param name="codeAction">Delegate executed when the code fix is chosen by the user.</param>
        public CodeFix( string title, Func<ICodeActionBuilder, Task> codeAction )
        {
            this.Title = title;
            this.CodeAction = codeAction;
        }
    }
}