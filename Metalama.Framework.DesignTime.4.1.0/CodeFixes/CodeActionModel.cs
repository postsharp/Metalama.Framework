// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes
{
    /// <summary>
    /// Represent a leaf in a code action menu.
    /// </summary>
    public abstract class CodeActionModel : CodeActionBaseModel
    {
        protected CodeActionModel( string title ) : base( title ) { }

        protected CodeActionModel() { }

        /// <summary>
        /// Executes the code action. This method is invoked in the analysis process.
        /// </summary>
        public abstract Task<CodeActionResult> ExecuteAsync( CodeActionExecutionContext executionContext, CancellationToken cancellationToken );

        public override ImmutableArray<CodeAction> ToCodeActions( CodeActionInvocationContext invocationContext, string titlePrefix = "" )
        {
            var title = titlePrefix + this.Title;

            return ImmutableArray.Create( CodeAction.Create( title, ct => this.InvokeAsync( invocationContext, ct ) ) );
        }

        /// <summary>
        /// Invokes the implementation of the code action. This method is invoked from the user process, but the code action
        /// implementation runs in the analysis process. 
        /// </summary>
        private async Task<Solution> InvokeAsync( CodeActionInvocationContext invocationContext, CancellationToken cancellationToken )
        {
            // Execute the current code action locally or remotely. In case of remote execution, the code action is serialized.
            var result = await invocationContext.Service.ExecuteCodeActionAsync( invocationContext.ProjectId, this, cancellationToken );

            // Apply the result to the current solution.
            var project = invocationContext.Document.Project;

            return await result.ApplyAsync( project, invocationContext.Logger, true, cancellationToken );
        }
    }
}