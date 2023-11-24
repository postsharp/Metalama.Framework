// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Services;

// ReSharper disable ClassCanBeSealed.Global
/// <summary>
/// A collection of service factories that are typically used to substitute production implementation of services
/// with test implementations. Actually a mutable wrapper around the immutable <see cref="ServiceProvider{T}"/>.
/// </summary>
/// <typeparam name="TService">The base interface type.</typeparam>
public class ServiceProviderBuilder<TService>
    where TService : class
{
    private readonly List<Func<ServiceProvider<TService>, ServiceProvider<TService>>> _buildActions = [];

    public ServiceProviderBuilder() { }

    public ServiceProviderBuilder( ServiceProviderBuilder<TService> prototype )
    {
        this._buildActions.AddRange( prototype._buildActions );
    }

    public ServiceProviderBuilder( params TService[] services )
    {
        foreach ( var service in services )
        {
            this.Add( service );
        }
    }

    /// <summary>
    /// Adds a lazily-created service.
    /// </summary>
    public void Add<T>( Func<ServiceProvider<TService>, T> func, bool allowOverride = false )
        where T : class, TService
    {
        this._buildActions.Add( serviceProvider => serviceProvider.WithService( func( serviceProvider ), allowOverride ) );
    }

    /// <summary>
    /// Adds a service.
    /// </summary>
    /// <param name="service"></param>
    [PublicAPI]
    public void Add( TService service, bool allowOverride = false )
    {
        this._buildActions.Add( serviceProvider => serviceProvider.WithService( service, allowOverride ) );
    }

    /// <summary>
    /// Adds a service obtained by replacing the <see cref="ServiceProvider"/>.
    /// </summary>
    public void Add( Func<ServiceProvider<TService>, ServiceProvider<TService>> func ) => this._buildActions.Add( func );

    public ServiceProvider<TService> Build( ServiceProvider<TService> initial )
    {
        var current = initial;

        foreach ( var action in this._buildActions )
        {
            current = action( current );
        }

        return current;
    }
}