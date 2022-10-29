// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// Handles the compiler requests (currently only from source generators) for a specific project.
/// </summary>
public abstract class ProjectHandler : IDisposable
{
    protected IServiceProvider ServiceProvider { get; }

    protected IProjectOptions ProjectOptions { get; }

    public ProjectKey ProjectKey { get; }

    protected ILogger Logger { get; }

    protected ProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions, ProjectKey projectKey )
    {
        this.ServiceProvider = serviceProvider;
        this.ProjectOptions = projectOptions;
        this.ProjectKey = projectKey;
        this.PendingTasks = new TaskBag( this.Logger );
        this.Logger = this.ServiceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
    }

    public abstract SourceGeneratorResult GenerateSources( Compilation compilation, TestableCancellationToken cancellationToken );

    protected virtual void Dispose( bool disposing ) { }

    public void Dispose() => this.Dispose( true );

    public TaskBag PendingTasks { get; }
}