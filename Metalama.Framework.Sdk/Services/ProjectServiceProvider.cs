// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Services;

/// <summary>
/// Gives access to project-scoped services. A wrapper around <see cref="ServiceProvider{T}"/> for <see cref="IProjectService"/>.
/// </summary>
public readonly struct ProjectServiceProvider
{
    public ServiceProvider<IProjectService> Underlying { get; }

    public GlobalServiceProvider Global => this.Underlying.FindNext<IGlobalService>() ?? throw new InvalidOperationException();

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

    public static implicit operator ProjectServiceProvider( ServiceProvider<IProjectService> serviceProvider ) => new( serviceProvider );

    public static implicit operator ServiceProvider<IProjectService>( ProjectServiceProvider serviceProvider ) => serviceProvider.Underlying;

    public static implicit operator GlobalServiceProvider( ProjectServiceProvider serviceProvider ) => serviceProvider.Global;

    public ProjectServiceProvider WithService( IProjectService service, bool allowOverride = false ) => this.Underlying.WithService( service, allowOverride );

    public ServiceProvider<IProjectService> WithServices( params IProjectService[] services ) => this.Underlying.WithServices( services );

    public ServiceProvider<IProjectService> WithServices( IEnumerable<IProjectService> services ) => this.Underlying.WithServices( services );

    public override string ToString() => this.Underlying.ToString();
}