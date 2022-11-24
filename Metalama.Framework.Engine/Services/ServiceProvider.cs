// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Services
{
    public abstract class ServiceProvider
    {
        internal IServiceProvider? NextProvider { get; private protected set; }

        public ServiceProvider<T> FindNext<T>() where T : class
        {
            for ( var i = this.NextProvider as ServiceProvider; i != null; i = i.NextProvider as ServiceProvider )
            {
                if ( i is ServiceProvider<T> good )
                {
                    return good;
                }
            }

            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// An immutable implementation of <see cref="IServiceProvider"/> that will index services that implement the <typeparamref name="TBase"/> interface.
    /// When a service is added to a <see cref="ServiceProvider{TBase}"/>, an mapping is created between the type of this object and the object itself,
    /// but also between the type of any interface derived from <typeparamref name="TBase"/> and implemented by this object.
    /// </summary>
    public class ServiceProvider<TBase> : ServiceProvider, IServiceProvider<TBase>
        where TBase : class
    {
        // This field is not readonly because we use two-phase initialization to resolve the problem of cyclic dependencies.
        private ImmutableDictionary<Type, ServiceNode> _services;

        public static ServiceProvider<TBase> Empty { get; } = new();

        private ServiceProvider() : this( ImmutableDictionary<Type, ServiceNode>.Empty, null ) { }

        private ServiceProvider<TBase> Clone( ImmutableDictionary<Type, ServiceNode> services, IServiceProvider? nextProvider )
        {
            var clone = (ServiceProvider<TBase>) this.MemberwiseClone();
            clone._services = services;
            clone.NextProvider = nextProvider;

            return clone;
        }

        private protected ServiceProvider( ImmutableDictionary<Type, ServiceNode> services, IServiceProvider? nextProvider )
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
            var serviceNode = new ServiceNode( _ => implementation, interfaceType );

            return this.Clone( this._services.Add( interfaceType, serviceNode ), this.NextProvider );
        }

        private void AddService( ServiceNode service, ImmutableDictionary<Type, ServiceNode>.Builder builder, bool allowOverride )
        {
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

            var interfaces = service.ServiceType.GetInterfaces();

            foreach ( var interfaceType in interfaces )
            {
                if ( typeof(TBase).IsAssignableFrom( interfaceType ) && interfaceType != typeof(TBase) )
                {
                    CheckService( interfaceType );

                    builder[interfaceType] = service;
                }
            }

            for ( var cursorType = service.ServiceType;
                  cursorType != null && typeof(TBase).IsAssignableFrom( cursorType );
                  cursorType = cursorType.BaseType )
            {
                CheckService( cursorType );

                builder[cursorType] = service;
            }
        }

        /// <summary>
        /// Returns a new <see cref="ServiceProvider{TBase}"/> where a service have been added to the current <see cref="ServiceProvider{TBase}"/>.
        /// If the new service is already present in the current <see cref="ServiceProvider{TBase}"/>, it is replaced in the new <see cref="ServiceProvider{TBase}"/>.
        /// </summary>
        public ServiceProvider<TBase> WithService( TBase service, bool allowOverride = false )
            => this.WithService( new ServiceNode( _ => service, service.GetType() ), allowOverride );

        public ServiceProvider<TBase> TryWithService<T>( Func<ServiceProvider<TBase>, T> func )
            where T : class, TBase
            => this.GetService<T>() == null ? this.WithService( func( this ) ) : this;

        public ServiceProvider<TBase> WithLazyService<T>( Func<ServiceProvider<TBase>, T> func )
            where T : TBase
            => new( this._services.Add( typeof(T), new ServiceNode( sp => func( (ServiceProvider<TBase>) sp ), typeof(T) ) ), this.NextProvider );

        public ServiceProvider<TBase> WithExternalService<T>( T service )
            where T : notnull
            => new( this._services.Add( typeof(T), new ServiceNode( _ => service, typeof(T) ) ), this.NextProvider );

        object? IServiceProvider.GetService( Type serviceType ) => this.GetService( serviceType );

        /// <summary>
        /// Gets the implementation of a given service type.
        /// </summary>
        public object? GetService( Type serviceType ) => this.GetOwnService( serviceType ) ?? this.NextProvider?.GetService( serviceType );

        private object? GetOwnService( Type serviceType ) => this._services.TryGetValue( serviceType, out var instance ) ? instance.GetService( this ) : null;

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
        public ServiceProvider<TBase> WithNextProvider( IServiceProvider nextProvider ) => new( this._services, nextProvider );

        public T? GetService<T>()
            where T : class, TBase
            => (T?) this.GetService( typeof(T) );

        public override string ToString()
        {
            return $"ServiceProvider Entries={this._services.Count}";
        }

        private protected class ServiceNode
        {
            private readonly Func<IServiceProvider, object> _func;
            private object? _service;

            public Type ServiceType { get; }

            public ServiceNode( Func<IServiceProvider, object> func, Type serviceType )
            {
                this._func = func;
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
        }
    }
}