// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Services;

/// <summary>
/// A collection of service factories that are typically used to substitute production implementation of services
/// with test implementations. Actually a mutable wrapper around the immutable <see cref="ServiceProvider{T}"/>.
/// </summary>
/// <typeparam name="TService">The base interface type.</typeparam>
public class ServiceProviderBuilder<TService>
    where TService : class
{
    /// <summary>
    /// Gets the resulting <see cref="ServiceProvider"/>.
    /// </summary>
    public ServiceProvider<TService> ServiceProvider { get; private set; } = ServiceProvider<TService>.Empty;

    /// <summary>
    /// Adds a lazily-created service.
    /// </summary>
    public void Add<T>( Func<ServiceProvider<TService>, T> func )
        where T : class, TService
    {
        this.ServiceProvider = this.ServiceProvider.WithLazyService( func );
    }

    /// <summary>
    /// Adds a service.
    /// </summary>
    /// <param name="service"></param>
    public void Add( TService service )
    {
        this.ServiceProvider = this.ServiceProvider.WithService( service );
    }

    /// <summary>
    /// Adds a service obtained by replacing the <see cref="ServiceProvider"/>.
    /// </summary>
    public void Add( Func<ServiceProvider<TService>, ServiceProvider<TService>> func ) => this.ServiceProvider = func( this.ServiceProvider );
}