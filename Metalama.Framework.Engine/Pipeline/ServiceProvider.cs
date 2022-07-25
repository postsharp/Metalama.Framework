// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Engine.Metrics;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// An immutable implementation of <see cref="IServiceProvider"/> that will index services that implement the <see cref="IService"/> interface.
    /// When a service is added to a <see cref="ServiceProvider"/>, an mapping is created between the type of this object and the object itself,
    /// but also between the type of any interface derived from <see cref="IService"/> and implemented by this object.
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider? _nextProvider;

        // This field is not readonly because we use two-phase initialization to resolve the problem of cyclic dependencies.
        private readonly ImmutableDictionary<Type, ServiceNode> _services;

        public static ServiceProvider Empty { get; } =
            new ServiceProvider( ImmutableDictionary<Type, ServiceNode>.Empty, null ).WithMark( ServiceProviderMark.Other );

        private ServiceProvider( ImmutableDictionary<Type, ServiceNode> services, IServiceProvider? nextProvider )
        {
            this._services = services;
            this._nextProvider = nextProvider;
        }

        /// <summary>
        /// Returns a copy of the current <see cref="ServiceProvider"/> with specified diagnostic mark.
        /// </summary>
        public ServiceProvider WithMark( ServiceProviderMark mark ) => this.WithService( mark );

        private ServiceProvider WithService( ServiceNode service )
        {
            var builder = this._services.ToBuilder();

            AddService( service, builder );

            return new ServiceProvider( builder.ToImmutable(), this._nextProvider );
        }

        public ServiceProvider WithUntypedService( Type interfaceType, object implementation )
        {
            var serviceNode = new ServiceNode( _ => implementation, interfaceType );

            return new ServiceProvider( this._services.Add( interfaceType, serviceNode ), this._nextProvider );
        }

        private static void AddService( ServiceNode service, ImmutableDictionary<Type, ServiceNode>.Builder builder )
        {
            var interfaces = service.ServiceType.GetInterfaces();

            foreach ( var interfaceType in interfaces )
            {
                if ( typeof(IService).IsAssignableFrom( interfaceType ) && interfaceType != typeof(IService) )
                {
                    builder[interfaceType] = service;
                }
            }

            for ( var cursorType = service.ServiceType;
                  cursorType != null && typeof(IService).IsAssignableFrom( cursorType );
                  cursorType = cursorType.BaseType )
            {
                builder[cursorType] = service;
            }
        }

        /// <summary>
        /// Returns a new <see cref="ServiceProvider"/> where a service have been added to the current <see cref="ServiceProvider"/>.
        /// If the new service is already present in the current <see cref="ServiceProvider"/>, it is replaced in the new <see cref="ServiceProvider"/>.
        /// </summary>
        public ServiceProvider WithService( IService service ) => this.WithService( new ServiceNode( _ => service, service.GetType() ) );

        public ServiceProvider WithExternalService<T>( T service )
            where T : notnull
            => new( this._services.Add( typeof(T), new ServiceNode( _ => service, typeof(T) ) ), this._nextProvider );

        object? IServiceProvider.GetService( Type serviceType ) => this.GetService( serviceType ) ?? this._nextProvider?.GetService( serviceType );

        /// <summary>
        /// Gets the implementation of a given service type.
        /// </summary>
        private object? GetService( Type serviceType ) => this._services.TryGetValue( serviceType, out var instance ) ? instance.GetService( this ) : null;

        /// <summary>
        /// Returns a new <see cref="ServiceProvider"/> where some given services have been added to the current <see cref="ServiceProvider"/>.
        /// If some of the new services are already present in the current <see cref="ServiceProvider"/>, they are replaced in the new <see cref="ServiceProvider"/>.
        /// </summary>
        public ServiceProvider WithServices( IEnumerable<IService>? services )
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
        /// Returns a new <see cref="ServiceProvider"/> with a new lazily-initialized service registration. The service, when
        /// initialized, will be given the <see cref="ServiceProvider"/> that requests the service. However, the service instantiated
        /// at this moment will be shared by all service providers that have been derived from the service provider returned by this method.
        /// </summary>
        public ServiceProvider WithSharedLazyInitializedService<T>( Func<IServiceProvider, T> func )
            where T : IService
        {
            return this.WithService( new ServiceNode( serviceProvider => func( serviceProvider ), typeof(T) ) );
        }

        /// <summary>
        /// Returns a new <see cref="ServiceProvider"/> where some given services have been added to the current <see cref="ServiceProvider"/>.
        /// If some of the new services are already present in the current <see cref="ServiceProvider"/>, they are replaced in the new <see cref="ServiceProvider"/>.
        /// </summary>
        public ServiceProvider WithServices( IService service, params IService[] services ) => this.WithService( service ).WithServices( services );

        /// <summary>
        /// Adds the services that have the same scope as the project processing itself.
        /// </summary>
        /// <param name="metadataReferences">A list of resolved metadata references for the current project.</param>
        public ServiceProvider WithProjectScopedServices( IEnumerable<MetadataReference> metadataReferences )
            => this.WithProjectScopedServices( metadataReferences, null );

        public ServiceProvider WithProjectScopedServices( Compilation compilation ) => this.WithProjectScopedServices( compilation.References, compilation );

        private ServiceProvider WithProjectScopedServices(
            IEnumerable<MetadataReference> metadataReferences,
            Compilation? compilation )
        {
            // ReflectionMapperFactory cannot be a global service because it keeps a reference from compilations to types of the
            // user assembly. When we need to unload the user assembly, we first need to unload the ReflectionMapperFactory.
            var serviceProvider = this.WithServices(
                new ReflectionMapperFactory(),
                new AssemblyLocator( this, metadataReferences ) );

            serviceProvider = serviceProvider.WithService( new UserCodeInvoker( serviceProvider ) );
            serviceProvider = serviceProvider.WithService( new BuiltInSerializerFactoryProvider( serviceProvider ) );
            serviceProvider = serviceProvider.WithServices( new SyntaxSerializationService( serviceProvider ), new CompileTimeTypeFactory() );
            serviceProvider = serviceProvider.WithServices( new SystemTypeResolver( serviceProvider ) );
            serviceProvider = serviceProvider.WithSharedLazyInitializedService( sp => new SymbolClassificationService( sp ) );

            serviceProvider = serviceProvider.WithService(
                new SyntaxGenerationContextFactory( compilation ?? SyntaxGenerationContext.EmptyCompilation, serviceProvider ) );

            serviceProvider = serviceProvider.WithMetricProviders();

            return serviceProvider;
        }

        /// <summary>
        /// Sets or replaces the next service provider in a chain.
        /// </summary>
        /// <param name="nextProvider"></param>
        /// <remarks>
        /// When the current service provider fails to find a service, it will try to find it using the next provider in the chain.
        /// When the next service provider has been set before, it gets replaced.
        /// </remarks>
        public ServiceProvider WithNextProvider( IServiceProvider nextProvider ) => new( this._services, nextProvider );

        public override string ToString()
        {
            var mark = this.GetService<ServiceProviderMark>();

            return $"ServiceProvider Mark='{mark}', Entries={this._services.Count}";
        }

        private class ServiceNode
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
                        if ( this._service == null )
                        {
                            this._service = this._func( serviceProvider );
                        }
                    }
                }

                return this._service;
            }
        }
    }
}