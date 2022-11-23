using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Pipeline;

public class ServiceFactory<TBase>
{
    private readonly Dictionary<Type, ServiceNode> _factories = new();

    public void Add<T>( Func<ServiceProvider<TBase>, T> func )
        where T : class, TBase
        => this.Add( typeof(T), func );
        
    public void Add( Type serviceType, Func<ServiceProvider<TBase>, object> func )
    {
        var node = new ServiceNode( func, this );
        var interfaces = serviceType.GetInterfaces();

        foreach ( var interfaceType in interfaces )
        {
            if ( typeof(IService).IsAssignableFrom( interfaceType ) && interfaceType != typeof(IService) )
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

    public HashSet<TBase> GetAdditionalServices( ServiceProvider<TBase> serviceProvider )
    {
        var set = new HashSet<TBase>( this._factories.Count );
        foreach ( var pair in this._factories )
        {
            if ( serviceProvider.GetService( pair.Key ) == null )
            {
                set.Add(  (TBase) pair.Value.GetService( serviceProvider ) );
            }
        }

        return set;
    }

    public event Action<TBase>? ServiceCreated;

    private class ServiceNode
    {
        private ServiceFactory<TBase> _parent;
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

    public T? GetService<T>( ServiceProvider<TBase> serviceProvider ) where T : class, TBase
    {
        if ( !this._factories.TryGetValue( typeof(T), out var node ) )
        {
            return null;
        }

        return (T?) node.GetService( serviceProvider );
    }
}