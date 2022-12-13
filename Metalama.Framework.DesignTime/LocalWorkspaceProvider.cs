// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Threading;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// An implementation of <see cref="WorkspaceProvider"/> that expects the UI services to be in the same process and to call <see cref="TrySetWorkspace"/>.
/// </summary>
internal sealed class LocalWorkspaceProvider : WorkspaceProvider
{
    private readonly TaskCompletionSource<Workspace> _workspace = new();

    public LocalWorkspaceProvider( GlobalServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public override Task<Workspace> GetWorkspaceAsync( CancellationToken cancellationToken ) => this._workspace.Task.WithCancellation( cancellationToken );

    public void TrySetWorkspace( Workspace workspace ) => this._workspace.TrySetResult( workspace );
}