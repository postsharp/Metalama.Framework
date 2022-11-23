// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Services;

public readonly struct GlobalServiceProvider
{
    public ServiceProvider<IGlobalService> Underlying { get; }

    private GlobalServiceProvider( ServiceProvider<IGlobalService> serviceProvider )
    {
        this.Underlying = serviceProvider;
    }

    public T GetRequiredService<T>() where T : class, IGlobalService
        => this.Underlying.GetService<T>() ?? throw new InvalidOperationException( $"Cannot get the service {typeof(T).Name}." );

    public T? GetService<T>() where T : class, IGlobalService => this.Underlying.GetService<T>();

    public ILoggerFactory GetLoggerFactory() => this.Underlying.GetLoggerFactory();

    public static implicit operator GlobalServiceProvider( ServiceProvider<IGlobalService> serviceProvider ) => new( serviceProvider );

    public static implicit operator ServiceProvider<IGlobalService>( GlobalServiceProvider serviceProvider ) => serviceProvider.Underlying;

    public T GetRequiredBackstageService<T>() where T : class, IBackstageService => this.Underlying.GetRequiredBackstageService<T>();

    public T? GetBackstageService<T>() where T : class, IBackstageService => this.Underlying.GetBackstageService<T>();

    public GlobalServiceProvider WithService( IGlobalService service ) => this.Underlying.WithService( service );

    public GlobalServiceProvider WithServices( params IGlobalService[] services ) => this.Underlying.WithServices( services );
}