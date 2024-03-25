// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Services;

/// <summary>
/// An immutable implementation of <see cref="IServiceProvider"/> that will index services that implement the <typeparamref name="TBase"/> interface.
/// When a service is added to a <see cref="ServiceProvider{TBase}"/>, an mapping is created between the type of this object and the object itself,
/// but also between the type of any interface derived from <typeparamref name="TBase"/> and implemented by this object.
/// </summary>
[PublicAPI]
public sealed class ServiceProvider<TBase> : ServiceProvider, IServiceProvider<TBase>
    where TBase : class
{
    // This field is not readonly because we use two-phase initialization to resolve the problem of cyclic dependencies.
    private ImmutableDictionary<Type, ServiceNode> _services;
    private Dictionary<Type, ServiceNode>? _servicesFast;

    public static ServiceProvider<TBase> Empty { get; } = new();

    private ServiceProvider() : this( ImmutableDictionary<Type, ServiceNode>.Empty, null ) { }

    private ServiceProvider<TBase> Clone( ImmutableDictionary<Type, ServiceNode> services, IServiceProvider? nextProvider )
    {
        var clone = (ServiceProvider<TBase>) this.MemberwiseClone();
        clone._services = services;
        clone._servicesFast = null;
        clone.NextProvider = nextProvider;

        return clone;
    }

    private ServiceProvider( ImmutableDictionary<Type, ServiceNode> services, IServiceProvider? nextProvider )
    {
        this._services = services;
        this.NextProvider = nextProvider;
    }

    private ServiceProvider<TBase> WithService( ServiceNode service, bool allowOverride )
    {
        var builder = this._services.ToBuilder();

        this.AddService( service, builder, allowOverride );

        return this.Clone( builder.ToImmutable(), this.NextProvider );
    }

    public ServiceProvider<TBase> WithUntypedService( Type interfaceType, object implementation )
    {
        var serviceNode = new ServiceNode( interfaceType, implementation );

        return this.Clone( this._services.Add( interfaceType, serviceNode ), this.NextProvider );
    }

    private void AddService( ServiceNode service, ImmutableDictionary<Type, ServiceNode>.Builder builder, bool allowOverride )
    {
#if DEBUG
        void CheckService( Type interfaceType )
        {
            if ( !allowOverride )
            {
                if ( builder.ContainsKey( interfaceType ) || this.NextProvider?.GetService( interfaceType ) != null )
                {
                    throw new InvalidOperationException( $"The service provider already contains the service '{interfaceType.Name}'." );
                }
            }
        }
#endif

        var interfaces = service.ServiceType.GetInterfaces();

        foreach ( var interfaceType in interfaces )
        {
            if ( typeof(TBase).IsAssignableFrom( interfaceType ) && interfaceType != typeof(TBase) )
            {
#if DEBUG
                CheckService( interfaceType );
#endif

                builder[interfaceType] = service;
            }
        }

        for ( var cursorType = service.ServiceType;
              cursorType != null && typeof(TBase).IsAssignableFrom( cursorType );
              cursorType = cursorType.BaseType )
        {
#if DEBUG
            CheckService( cursorType );
#endif

            builder[cursorType] = service;
        }
    }

    /// <summary>
    /// Returns a new <see cref="ServiceProvider{TBase}"/> where a service have been added to the current <see cref="ServiceProvider{TBase}"/>.
    /// If the new service is already present in the current <see cref="ServiceProvider{TBase}"/>, it is replaced in the new <see cref="ServiceProvider{TBase}"/>.
    /// </summary>
    public ServiceProvider<TBase> WithService( TBase service, bool allowOverride = false )
        => this.WithService( new ServiceNode( service.GetType(), service ), allowOverride );

    public ServiceProvider<TBase> WithServiceConditional<T>( Func<ServiceProvider<TBase>, T> func )
        where T : class, TBase
        => this.GetService<T>() == null ? this.WithService( func( this ) ) : this;

    public ServiceProvider<TBase> WithService<T>( Func<ServiceProvider<TBase>, T> func )
        where T : class, TBase
        => this.WithService( func( this ) );

    object? IServiceProvider.GetService( Type serviceType ) => this.GetService( serviceType );

    /// <summary>
    /// Gets the implementation of a given service type.
    /// </summary>
    public object? GetService( Type serviceType )
    {
        // We use the ImmutableDictionary to build the ServiceProvider, but to consume services, we use a Dictionary
        // which has a O(1) access time instead of O(log(n)). The Dictionary will be used in a read-only manner only.
        // Data races in instantiating this Dictionary do not matter.
        this._servicesFast ??= new Dictionary<Type, ServiceNode>( this._services );

        if ( this._servicesFast!.TryGetValue( serviceType, out var serviceNode ) )
        {
            return serviceNode.GetService( this );
        }
        else
        {
            return this.NextProvider?.GetService( serviceType );
        }
    }

    /// <summary>
    /// Returns a new <see cref="ServiceProvider{TBase}"/> where some given services have been added to the current <see cref="ServiceProvider{TBase}"/>.
    /// If some of the new services are already present in the current <see cref="ServiceProvider{TBase}"/>, they are replaced in the new <see cref="ServiceProvider{TBase}"/>.
    /// </summary>
    public ServiceProvider<TBase> WithServices( IEnumerable<TBase>? services )
    {
        if ( services == null )
        {
            return this;
        }

        var provider = this;

        foreach ( var s in services )
        {
            provider = provider.WithService( s );
        }

        return provider;
    }

    /// <summary>
    /// Returns a new <see cref="ServiceProvider{TBase}"/> where some given services have been added to the current <see cref="ServiceProvider{TBase}"/>.
    /// If some of the new services are already present in the current <see cref="ServiceProvider{TBase}"/>, they are replaced in the new <see cref="ServiceProvider{TBase}"/>.
    /// </summary>
    public ServiceProvider<TBase> WithServices( TBase service, params TBase[] services ) => this.WithService( service ).WithServices( services );

    /// <summary>
    /// Sets or replaces the next service provider in a chain.
    /// </summary>
    /// <param name="nextProvider"></param>
    /// <remarks>
    /// When the current service provider fails to find a service, it will try to find it using the next provider in the chain.
    /// When the next service provider has been set before, it gets replaced.
    /// </remarks>
    internal ServiceProvider<TBase> WithNextProvider( IServiceProvider nextProvider ) => new( this._services, nextProvider );

    public T? GetService<T>()
        where T : class, TBase
        => (T?) this.GetService( typeof(T) );

    public override string ToString() => $"ServiceProvider Entries={this._services.Count}";

    public override void Dispose()
    {
        foreach ( var serviceNode in this._services.Values )
        {
            serviceNode.Dispose();
        }

        if ( this.NextProvider is IDisposable disposable )
        {
            disposable.Dispose();
        }
    }

    private sealed class ServiceNode
    {
        private readonly Func<IServiceProvider, object>? _func;
        private object? _service;

        public Type ServiceType { get; }

        public ServiceNode( Type serviceType, Func<IServiceProvider, object> func )
        {
            this._func = func;
            this.ServiceType = serviceType;
        }

        public ServiceNode( Type serviceType, object service )
        {
            this._func = null;
            this._service = service;
            this.ServiceType = serviceType;
        }

        public object GetService( IServiceProvider serviceProvider )
        {
            if ( this._service == null )
            {
                lock ( this )
                {
                    this._service ??= this._func( serviceProvider );
                }
            }

            return this._service;
        }

        public void Dispose()
        {
            var disposable = this._service as IDisposable;
            disposable?.Dispose();
        }
    }
}