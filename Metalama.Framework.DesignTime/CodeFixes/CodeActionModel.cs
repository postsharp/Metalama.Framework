// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        // TODO: SourceAssemblyName and SourceRedistributionLicenseKey can be encapsulated to a record.
        internal string? SourceAssemblyName { get; }

        internal string? SourceRedistributionLicenseKey { get; }

        protected CodeActionModel( string title, string? sourceAssemblyName, string? sourceRedistributionLicenseKey ) : base( title )
        {
            this.SourceAssemblyName = sourceAssemblyName;
            this.SourceRedistributionLicenseKey = sourceRedistributionLicenseKey;
        }

        // Deserialization constructor.
        protected CodeActionModel() { }

        /// <summary>
        /// Executes the code action. This method is invoked in the analysis process.
        /// </summary>
        public abstract Task<CodeActionResult> ExecuteAsync( CodeActionExecutionContext executionContext, CancellationToken cancellationToken );

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

            // Apply the result to the current solution.
            var project = invocationContext.Document.Project;

            return await result.ApplyAsync( project, invocationContext.Logger, true, cancellationToken );
        }
    }
}