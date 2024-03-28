// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Metalama.Framework.DesignTime.VisualStudio;

internal class VsUserProcessInvalidationService : IGlobalService
{
    // protected Task RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind kind, Solution oldSolution, Solution newSolution, ProjectId projectId = null, DocumentId documentId = null)
    private static readonly MethodInfo _raiseWorkspaceChangedEventAsync =
        typeof(Workspace).GetMethod( "RaiseWorkspaceChangedEventAsync", BindingFlags.Instance | BindingFlags.NonPublic ).AssertNotNull();

    private readonly WorkspaceProvider _workspaceProvider;

    public VsUserProcessInvalidationService( GlobalServiceProvider serviceProvider )
    {
        this._workspaceProvider = serviceProvider.GetRequiredService<WorkspaceProvider>();
        var hub = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
        hub.CompilationResultChanged += this.OnCompilationResultChanged;
        
        // TODO: We probable should not invalidate analyzer results immediately because the analysis engine may
        // request the changes anyway, and our strategy would result in performance degradation. Instead, we could
        // invalidate the results if the IDE has not requested new analysis results a few seconds after they are known
        // to be available. This strategy is more complex and would require to rely on a different event than CompilationResultChanged.
    }

    private async void OnCompilationResultChanged( CompilationResultChangedEventArgs args )
    {
        if ( !this._workspaceProvider.TryGetWorkspace( out var workspace ) )
        {
            // The workspace is not available yet.
            return;
        }

        var project = await this._workspaceProvider.GetProjectAsync( args.ProjectKey, default );

        foreach ( var path in args.SyntaxTreePaths )
        {
            foreach ( var documentId in workspace.CurrentSolution.GetDocumentIdsWithFilePath( path ) )
            {
                var document = project.GetDocument( documentId );

                if ( document == null )
                {
                    // The document is not in the current project.
                    continue;
                }

                // TODO: Generate a compiled expression.
                _raiseWorkspaceChangedEventAsync.Invoke(
                    workspace,
                    [WorkspaceChangeKind.DocumentChanged, workspace.CurrentSolution, workspace.CurrentSolution, project.Id, documentId] );
            }
        }
    }
}