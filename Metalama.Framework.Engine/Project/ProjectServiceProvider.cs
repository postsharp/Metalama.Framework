using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using System;

namespace Metalama.Framework.Project;

public readonly struct ProjectServiceProvider
{
    public ServiceProvider<IProjectService> Underlying { get; }

    public GlobalServiceProvider Global => (ServiceProvider<IService>) this.Underlying.NextProvider.AssertNotNull();

    private ProjectServiceProvider( ServiceProvider<IProjectService> serviceProvider )
    {
        this.Underlying = serviceProvider;
    }

    public T GetRequiredService<T>() where T : class, IProjectService
        => this.Underlying.GetService<T>() ?? throw new InvalidOperationException( $"Cannot get the service {typeof(T).Name}." );

    public T? GetService<T>() where T : class, IProjectService => this.Underlying.GetService<T>();

    public ILoggerFactory GetLoggerFactory() => this.Underlying.GetLoggerFactory();

    public static implicit operator ProjectServiceProvider( ServiceProvider<IProjectService> serviceProvider ) => new ProjectServiceProvider( serviceProvider );

    public static implicit operator ServiceProvider<IProjectService>( ProjectServiceProvider serviceProvider ) => serviceProvider.Underlying;

    public static implicit operator GlobalServiceProvider( ProjectServiceProvider serviceProvider ) => serviceProvider.Global;

    public ProjectServiceProvider WithService( IProjectService service ) => this.Underlying.WithService( service );

    public ServiceProvider<IProjectService> WithServices( params IProjectService[] services ) => this.Underlying.WithServices( services );
}
    