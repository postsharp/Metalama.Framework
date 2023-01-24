﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Services;

internal abstract class WorkspaceProvider : IGlobalService, IDisposable
{
    private readonly TimeBasedCache<ProjectKey, ProjectId> _projectKeyToProjectIdMap = new( TimeSpan.FromMinutes( 10 ) );

    protected ILogger Logger { get; }

    protected WorkspaceProvider( GlobalServiceProvider serviceProvider )
    {
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "WorkspaceProvider" );
    }

    protected abstract Task<Workspace> GetWorkspaceAsync( CancellationToken cancellationToken = default );

    public async ValueTask<Microsoft.CodeAnalysis.Project?> GetProjectAsync( ProjectKey projectKey, CancellationToken cancellationToken )
    {
        var workspace = await this.GetWorkspaceAsync( cancellationToken );

        if ( !this._projectKeyToProjectIdMap.TryGetValue( projectKey, out var projectId ) )
        {
            foreach ( var project in workspace.CurrentSolution.Projects )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( project.AssemblyName != projectKey.AssemblyName )
                {
                    continue;
                }

                var thisProjectKey = ProjectKeyFactory.FromProject( project );

                if ( thisProjectKey == null )
                {
                    // This is not a C# project.
                    continue;
                }

                this._projectKeyToProjectIdMap.TryAdd( thisProjectKey, project.Id );

                if ( thisProjectKey == projectKey )
                {
                    return project;
                }
            }

            // Error: the compilation could not be found.
            this.Logger.Warning?.Log( $"Cannot find a project in the workspace for '{projectKey}'." );

            return default;
        }
        else
        {
            return workspace.CurrentSolution.GetProject( projectId );
        }
    }

    public async ValueTask<Compilation?> GetCompilationAsync( ProjectKey projectKey, CancellationToken cancellationToken = default )
    {
        var project = await this.GetProjectAsync( projectKey, cancellationToken );

        if ( project == null )
        {
            return null;
        }

        if ( !project.TryGetCompilation( out var compilation ) )
        {
            compilation = await project.GetCompilationAsync( cancellationToken );
        }

        return compilation;
    }

    public virtual void Dispose()
    {
        this._projectKeyToProjectIdMap.Dispose();
    }
}