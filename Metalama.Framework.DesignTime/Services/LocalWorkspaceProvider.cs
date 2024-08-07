// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;
using RoslynProject = Microsoft.CodeAnalysis.Project;

namespace Metalama.Framework.DesignTime.Services;

/// <summary>
/// An implementation of <see cref="WorkspaceProvider"/> that expects the UI services to be in the same process and to call <see cref="TrySetWorkspace"/>.
/// </summary>
public sealed class LocalWorkspaceProvider : WorkspaceProvider
{
    private readonly TaskCompletionSource<Workspace> _workspace = new();

    public LocalWorkspaceProvider( GlobalServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public override Task<Workspace> GetWorkspaceAsync( CancellationToken cancellationToken = default )
    {
        if ( !this._workspace.Task.IsCompleted )
        {
            this.Logger.Warning?.Log( $"The workspace is not yet available. Waiting." );
        }

        return this._workspace.Task.WithCancellation( cancellationToken );
    }

    internal void TrySetWorkspace( Workspace workspace ) => this._workspace.TrySetResult( workspace );

}

public sealed class SourceGeneratorTouchFileWatcher( GlobalServiceProvider serviceProvider ) : IGlobalService
{
    private readonly IProjectOptionsFactory _projectOptionsFactory = serviceProvider.GetRequiredService<IProjectOptionsFactory>();
    private readonly WorkspaceProvider _workspaceProvider = serviceProvider.GetRequiredService<WorkspaceProvider>();
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();

    private Workspace _workspace = null!;

    public async Task StartAsync()
    {
        // TODO: need to figure out how to get the workspace here
        // probably use WorkspaceProvider, ignoring remote workspaces and overriding it in VS to get VisualStudioWorkspace using VS APIs
        var workspace = await this._workspaceProvider.GetWorkspaceAsync();

        // TODO: is this necessary for all IDEs?
        Invariant.Assert( this._workspace == null );
        this._workspace = workspace;

        workspace.WorkspaceChanged += this.OnWorkspaceChanged;

        foreach ( var project in workspace.CurrentSolution.Projects )
        {
            await this.ProcessProjectAsync( project );
        }
    }

    private void OnWorkspaceChanged( object? sender, WorkspaceChangeEventArgs e )
    {
        if ( e.Kind is WorkspaceChangeKind.ProjectAdded or WorkspaceChangeKind.ProjectReloaded )
        {
            var project = e.NewSolution.GetProject( e.ProjectId )!;
            _ = this.ProcessProjectAsync( project );
        }

        // TODO: stop and remove watchers for removed projects
    }

    // TODO: explain
    private async Task ProcessProjectAsync( RoslynProject project )
    {
        try
        {
            var projectOptions = this._projectOptionsFactory.GetProjectOptions( project );
            var touchFilePath = projectOptions.SourceGeneratorTouchFile;

            if ( Path.GetDirectoryName( touchFilePath ) is not { } touchFileDirectory
                || Path.GetFileName( touchFilePath ) is not { } touchFileName )
            {
                return;
            }

            var fileWatcher = new FileSystemWatcher( touchFileDirectory, touchFileName );

            if ( !this._watchers.TryAdd( touchFilePath!, fileWatcher ) )
            {
                return;
            }

            var touchFileChanged = new TaskCompletionSource<bool>();

            fileWatcher.Created += ( s, e ) => touchFileChanged.TrySetResult( true );
            fileWatcher.Changed += ( s, e ) => touchFileChanged.TrySetResult( true );

            fileWatcher.EnableRaisingEvents = true;

            while ( true )
            {
                await touchFileChanged.Task;

                using var touchFileStream = File.OpenRead( touchFilePath! );

                touchFileChanged = new TaskCompletionSource<bool>();

                var solution = this._workspace.CurrentSolution;

                var documentIds = solution.GetDocumentIdsWithFilePath( touchFilePath );

                foreach ( var documentId in documentIds )
                {
                    var document = solution.GetAdditionalDocument( documentId );

                    if ( document == null )
                    {
                        continue;
                    }

                    var text = await document.GetTextAsync();

                    solution = solution.WithAdditionalDocumentText( documentId, SourceText.From( touchFileStream, text.Encoding, text.ChecksumAlgorithm, canBeEmbedded: text.CanBeEmbedded ) );
                }

                Invariant.Assert( this._workspace.TryApplyChanges( solution ) );
            }
        }
        catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
        {
            DesignTimeExceptionHandler.ReportException( e );
        }
    }
}