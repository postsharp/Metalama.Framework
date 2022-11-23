// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Services;

public readonly struct ProjectServiceProvider
{
    public ServiceProvider<IProjectService> Underlying { get; }

    public GlobalServiceProvider Global =>this.Underlying.FindNext<IGlobalService>().AssertNotNull();

    private ProjectServiceProvider( ServiceProvider<IProjectService> serviceProvider )
    {
        this.Underlying = serviceProvider;
    }

    public T GetRequiredService<T>()
        where T : class, IProjectService
        => this.Underlying.GetService<T>() ?? throw new InvalidOperationException( $"Cannot get the service {typeof(T).Name}." );

    public T? GetService<T>()
        where T : class, IProjectService
        => this.Underlying.GetService<T>();

    public ILoggerFactory GetLoggerFactory() => this.Underlying.GetLoggerFactory();

    public static implicit operator ProjectServiceProvider( ServiceProvider<IProjectService> serviceProvider ) => new( serviceProvider );

    public static implicit operator ServiceProvider<IProjectService>( ProjectServiceProvider serviceProvider ) => serviceProvider.Underlying;

    public static implicit operator GlobalServiceProvider( ProjectServiceProvider serviceProvider ) => serviceProvider.Global;

    public ProjectServiceProvider WithService( IProjectService service, bool allowOverride = false  ) => this.Underlying.WithService( service, allowOverride );

    public ServiceProvider<IProjectService> WithServices( params IProjectService[] services ) => this.Underlying.WithServices( services );

    /// <summary>
    /// Gets the global <see cref="ReferenceAssemblyLocator"/>, but initialize it with the current <see cref="ProjectServiceProvider"/> if it has not
    /// been initialized yet.
    /// </summary>
    internal ReferenceAssemblyLocator GetReferenceAssemblyLocator() => this.Global.GetRequiredService<ReferenceAssemblyLocatorProvider>().GetInstance( this );
}