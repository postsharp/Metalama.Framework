// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// Handles the compiler requests (currently only from source generators) for a specific project.
/// </summary>
public abstract class ProjectHandler : IDisposable
{
    private readonly ITaskRunner _taskRunner;

    protected GlobalServiceProvider ServiceProvider { get; }

    protected IProjectOptions ProjectOptions { get; }

    protected ProjectKey ProjectKey { get; }

    protected ILogger Logger { get; }

    /// <summary>
    /// Gets an override of touch id for the current project, for the cases when the IDE is not watching the touch file for changes.
    /// </summary>
    /// <remarks>
    /// This exists for two reasons:
    /// 1. Roslyn in VS has a bug (https://github.com/dotnet/roslyn/issues/74716) where the touch file is not correctly watched for changes.
    /// 2. Rider does not currently watch the touch file for changes (https://youtrack.jetbrains.com/issue/RIDER-75959).
    /// 
    /// Note that this approach can't trigger running the generator, which means that changes will become visible only once the user types something in some C# file in the project.
    /// 
    /// Also, it only works in the current process.
    /// </remarks>
    internal string? TouchIdOverride { get; private protected set; }

    protected ProjectHandler( GlobalServiceProvider serviceProvider, IProjectOptions projectOptions, ProjectKey projectKey )
    {
        this.ServiceProvider = serviceProvider;
        this.ProjectOptions = projectOptions;
        this.ProjectKey = projectKey;
        this.Logger = this.ServiceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
        this.PendingTasks = new TaskBag( this.Logger );
        this._taskRunner = this.ServiceProvider.GetRequiredService<ITaskRunner>();
    }

    public abstract SourceGeneratorResult GenerateSources( Compilation compilation, TestableCancellationToken cancellationToken );

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            this._taskRunner.RunSynchronously( this.PendingTasks.WaitAllAsync );
        }
    }

    public void Dispose() => this.Dispose( true );

    public TaskBag PendingTasks { get; }
}