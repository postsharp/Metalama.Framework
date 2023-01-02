// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

public abstract class WorkspaceProvider : IGlobalService, IDisposable
{
    private readonly TimeBasedCache<ProjectKey, ProjectId> _projectKeyToProjectIdMap = new( TimeSpan.FromMinutes( 10 ) );

    public ILogger Logger { get; }

    protected WorkspaceProvider( GlobalServiceProvider serviceProvider )
    {
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "WorkspaceProvider" );
    }

    public abstract Task<Workspace> GetWorkspaceAsync( CancellationToken cancellationToken = default );

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

                if ( !project.TryGetCompilation( out var compilation ) )
                {
                    compilation = await project.GetCompilationAsync( cancellationToken );

                    if ( compilation == null )
                    {
                        this.Logger.Warning?.Log( $"Cannot get the compilation for project '{project.Id}'." );

                        continue;
                    }
                }

                var thisProjectKey = ProjectKeyFactory.FromCompilation( compilation );

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

    public async ValueTask<Compilation?> GetCompilationAsync( ProjectKey projectKey, CancellationToken cancellationToken )
    {
        var workspace = await this.GetWorkspaceAsync( cancellationToken );

        if ( !this._projectKeyToProjectIdMap.TryGetValue( projectKey, out var projectId ) )
        {
            foreach ( var project in workspace.CurrentSolution.Projects )
            {
                if ( project.AssemblyName != projectKey.AssemblyName )
                {
                    this.Logger.Warning?.Log( $"Cannot get the compilation for project '{project.Id}'." );

                    continue;
                }

                if ( !project.TryGetCompilation( out var compilation ) )
                {
                    compilation = await project.GetCompilationAsync( cancellationToken );

                    if ( compilation == null )
                    {
                        continue;
                    }
                }

                var thisProjectKey = ProjectKeyFactory.FromCompilation( compilation );

                this._projectKeyToProjectIdMap.TryAdd( thisProjectKey, project.Id );

                if ( thisProjectKey == projectKey )
                {
                    return compilation;
                }
            }

            // Error: the compilation could not be found.
            this.Logger.Warning?.Log( $"Cannot find a project in the workspace for '{projectKey}'." );

            return default;
        }
        else
        {
            var project = workspace.CurrentSolution.GetProject( projectId );

            if ( project == null )
            {
                // Error: the project could not be found.
                this.Logger.Warning?.Log( $"The project '{projectKey}' no longer exists in the workspace." );

                return default;
            }

            if ( !project.TryGetCompilation( out var compilation ) )
            {
                compilation = await project.GetCompilationAsync( cancellationToken );
            }

            return compilation;
        }
    }

    public virtual void Dispose()
    {
        this._projectKeyToProjectIdMap.Dispose();
    }
}