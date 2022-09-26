// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
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
        protected CodeActionModel( string title) : base( title ) { }

        // Deserialization constructor.
        protected CodeActionModel() { }

        /// <summary>
        /// Executes the code action. This method is invoked in the analysis process.
        /// </summary>
        public abstract Task<CodeActionResult> ExecuteAsync(
            CodeActionExecutionContext executionContext,
            bool isComputingPreview,
            CancellationToken cancellationToken );

        public override ImmutableArray<CodeAction> ToCodeActions( CodeActionInvocationContext invocationContext, string titlePrefix = "" )
        {
            var title = titlePrefix + this.Title;

            return ImmutableArray.Create( LamaCodeAction.Create( title, ( p, ct ) => this.InvokeAsync( invocationContext, p, ct ) ) );
        }

        /// <summary>
        /// Invokes the implementation of the code action. This method is invoked from the user process, but the code action
        /// implementation runs in the analysis process. 
        /// </summary>
        private async Task<Solution> InvokeAsync( CodeActionInvocationContext invocationContext, bool computingPreview, CancellationToken cancellationToken )
        {
            // Execute the current code action locally or remotely. In case of remote execution, the code action is serialized.
            var result = await invocationContext.Service.ExecuteCodeActionAsync( invocationContext.ProjectKey, this, computingPreview, cancellationToken );

            if ( result.IsSuccess )
            {
                // Apply the result to the current solution.
                var project = invocationContext.Document.Project;

                return await result.ApplyAsync( project, invocationContext.Logger, true, cancellationToken );
            }
            else
            {
                return await CodeFixHelper.ReportErrorsAsCommentsAsync(
                    invocationContext.SyntaxNode,
                    invocationContext.Document,
                    result.ErrorMessages.AssertNotNull(),
                    cancellationToken );
            }
        }
    }
}