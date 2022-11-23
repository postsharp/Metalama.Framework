// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Pipeline;
using System;

namespace Metalama.Framework.Project;

public readonly struct GlobalServiceProvider
{
    public ServiceProvider<IService> Underlying { get; }

    private GlobalServiceProvider( ServiceProvider<IService> serviceProvider )
    {
        this.Underlying = serviceProvider;
    }

    public T GetRequiredService<T>() where T : class, IService
        => this.Underlying.GetService<T>() ?? throw new InvalidOperationException( $"Cannot get the service {typeof(T).Name}." );

    public T? GetService<T>() where T : class, IService => this.Underlying.GetService<T>();

    public ILoggerFactory GetLoggerFactory() => this.Underlying.GetLoggerFactory();

    public static implicit operator GlobalServiceProvider( ServiceProvider<IService> serviceProvider ) => new( serviceProvider );

    public static implicit operator ServiceProvider<IService>( GlobalServiceProvider serviceProvider ) => serviceProvider.Underlying;

    public T GetRequiredBackstageService<T>() where T : class, IBackstageService => this.Underlying.GetRequiredBackstageService<T>();

    public T? GetBackstageService<T>() where T : class, IBackstageService => this.Underlying.GetBackstageService<T>();

    public GlobalServiceProvider WithService( IService service ) => this.Underlying.WithService( service );

    public GlobalServiceProvider WithServices( params IService[] services ) => this.Underlying.WithServices( services );
}