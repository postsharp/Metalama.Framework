// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Services;

/// <summary>
/// A collection of service factories that are typically used to substitute production implementation of services
/// with test implementations.
/// </summary>
/// <typeparam name="TService"></typeparam>
public class ServiceProviderBuilder<TService>
    where TService : class
{
    public ServiceProvider<TService> ServiceProvider { get; private set; } = ServiceProvider<TService>.Empty;

    public void Add<T>( Func<ServiceProvider<TService>, T> func )
        where T : class, TService
    {
        this.ServiceProvider = this.ServiceProvider.WithLazyService( func );
    }

    public void Add( TService service )
    {
        this.ServiceProvider = this.ServiceProvider.WithService( service );
    }
}