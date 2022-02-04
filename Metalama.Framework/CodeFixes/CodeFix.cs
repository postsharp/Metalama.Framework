// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.CodeFixes
{
    /// <summary>
    /// Represents a modification of the current solution, including the <see cref="Title"/> of transformation.
    /// To create instantiate this class, use <see cref="CodeFixFactory"/>.
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
        internal Func<ICodeFixBuilder, Task> Action { get; }

        internal CodeFix( string title, Func<ICodeFixBuilder, Task> action )
        {
            this.Title = title;
            this.Action = action;
        }

        /// <summary>
        /// Suggests the current code fix for a given <see cref="IDiagnosticLocation"/> (typically for a declaration or syntax node). 
        /// </summary>
        /// <param name="location">The declaration or node where the code fix should be suggested.</param>
        /// <param name="sink">A <see cref="ScopedDiagnosticSink"/>.</param>
        public void SuggestFor( IDiagnosticLocation location, IDiagnosticSink sink ) => sink.Suggest( location, this );

        /// <summary>
        /// Suggests the current code fix for the default location (typically a declaration or syntax node) in the current context.
        /// </summary>
        /// <param name="sink">The <see cref="ScopedDiagnosticSink"/> for the current context.</param>
        public void SuggestFor( in ScopedDiagnosticSink sink ) => sink.Suggest( this );
    }
}