// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Services;

/// <summary>
/// A collection of service factories that are typically used to substitute production implementation of services
/// with test implementations.
/// </summary>
/// <typeparam name="TBase"></typeparam>
public class ServiceFactory<TBase>
    where TBase : class
{
    private readonly Dictionary<Type, ServiceNode> _factories = new();

    /// <summary>
    /// Adds a strongly typed service.
    /// </summary>
    /// <param name="func">A function that returns the service implementation.</param>
    /// <typeparam name="T">The service interface.</typeparam>
    public void Add<T>( Func<ServiceProvider<TBase>, T> func )
        where T : class, TBase
        => this.Add( typeof(T), func );

    /// <summary>
    /// Adds a weakly typed service.
    /// </summary>
    /// <param name="serviceType">The service interface.</param>
    /// <param name="func">A function that returns the service implementation.</param>
    public void Add( Type serviceType, Func<ServiceProvider<TBase>, object> func )
    {
        var node = new ServiceNode( func, this );
        var interfaces = serviceType.GetInterfaces();

        foreach ( var interfaceType in interfaces )
        {
            if ( typeof(IGlobalService).IsAssignableFrom( interfaceType ) && interfaceType != typeof(IGlobalService) )
            {
                this._factories[interfaceType] = node;
            }
        }

        for ( var cursorType = serviceType;
              cursorType != null && typeof(TBase).IsAssignableFrom( cursorType );
              cursorType = cursorType.BaseType )
        {
            this._factories[cursorType] = node;
        }
    }

    /// <summary>
    /// Instantiates and returns the services of the current <see cref="ServiceFactory{TBase}"/> that are not
    /// yet available in a given provider. 
    /// </summary>
    /// <param name="serviceProvider">A provider.</param>
    /// <returns>The set of created services.</returns>
    public HashSet<TBase> GetAdditionalServices( ServiceProvider<TBase> serviceProvider )
    {
        var set = new HashSet<TBase>();

        foreach ( var pair in this._factories )
        {
            if ( serviceProvider.GetService( pair.Key ) == null )
            {
                set.Add( (TBase) pair.Value.GetService( serviceProvider ) );
            }
        }

        return set;
    }

    /// <summary>
    /// Event raised when a service is instantiated.
    /// </summary>
    public event Action<TBase>? ServiceCreated;

    private class ServiceNode
    {
        private readonly ServiceFactory<TBase> _parent;
        private readonly Func<ServiceProvider<TBase>, object> _func;
        private object? _service;

        public ServiceNode( Func<ServiceProvider<TBase>, object> func, ServiceFactory<TBase> parent )
        {
            this._func = func;
            this._parent = parent;
        }

        public object GetService( ServiceProvider<TBase> serviceProvider )
        {
            if ( this._service == null )
            {
                lock ( this )
                {
                    this._service ??= this._func( serviceProvider );
                    this._parent.ServiceCreated?.Invoke( (TBase) this._service );
                }
            }

            return this._service;
        }
    }

    public T? GetService<T>( ServiceProvider<TBase> serviceProvider )
        where T : class, TBase
    {
        if ( !this._factories.TryGetValue( typeof(T), out var node ) )
        {
            return null;
        }

        return (T?) node.GetService( serviceProvider );
    }
}