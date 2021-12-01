// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using PostSharp.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
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
        private ImmutableDictionary<Type, Lazy<IService, Type>> _services;

        public static ServiceProvider Empty { get; } =
            new ServiceProvider( ImmutableDictionary<Type, Lazy<IService, Type>>.Empty, null ).WithMark( ServiceProviderMark.Other );

        private ServiceProvider( ImmutableDictionary<Type, Lazy<IService, Type>> services, IServiceProvider? nextProvider )
        {
            this._services = services;
            this._nextProvider = nextProvider;
        }

        /// <summary>
        /// Returns a copy of the current <see cref="ServiceProvider"/> with specified diagnostic mark.
        /// </summary>
        public ServiceProvider WithMark( ServiceProviderMark mark ) => this.WithService( mark );

        private ServiceProvider WithService( Lazy<IService, Type> service )
        {
            var builder = this._services.ToBuilder();

            AddService( service, builder );

            return new ServiceProvider( builder.ToImmutable(), this._nextProvider );
        }

        private static void AddService( Lazy<IService, Type> service, ImmutableDictionary<Type, Lazy<IService, Type>>.Builder builder )
        {
            var interfaces = service.Metadata.GetInterfaces();

            foreach ( var interfaceType in interfaces )
            {
                if ( typeof(IService).IsAssignableFrom( interfaceType ) && interfaceType != typeof(IService) )
                {
                    builder[interfaceType] = service;
                }
            }

            for ( var cursorType = service.Metadata;
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
        public ServiceProvider WithService( IService service )
        {
            var lazy = new Lazy<IService, Type>( () => service, service.GetType() );

            return this.WithService( lazy );
        }

        object? IServiceProvider.GetService( Type serviceType ) => this.GetService( serviceType );

        /// <summary>
        /// Gets the implementation of a given service type.
        /// </summary>
        public IService? GetService( Type serviceType )
        {
            if ( this._services.TryGetValue( serviceType, out var instance ) )
            {
                return instance.Value;
            }
            else
            {
                var service = this._nextProvider?.GetService( serviceType );

                if ( service == null )
                {
                    return null;
                }
                else
                {
                    return (IService) service;
                }
            }
        }

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
        /// Returns a new <see cref="ServiceProvider"/> where some given services have been added to the current <see cref="ServiceProvider"/>.
        /// If some of the new services are already present in the current <see cref="ServiceProvider"/>, they are replaced in the new <see cref="ServiceProvider"/>.
        /// </summary>
        public ServiceProvider WithServices( IService service, params IService[] services ) => this.WithService( service ).WithServices( services );

        /// <summary>
        /// Adds a set of services that depend on each other (additionally of depending on prior services in the current
        /// <see cref="ServiceProvider"/> instance) and returns the resulting immutable <see cref="ServiceProvider"/>.
        /// If some of the new services are already present in the current <see cref="ServiceProvider"/>, they are replaced in the new <see cref="ServiceProvider"/>. 
        /// </summary>
        internal ServiceProvider WithLateBoundServices( params LateBoundService[] services )
        {
            if ( services.Length == 0 )
            {
                return this;
            }

            // We need the reference to the new ServiceProvider before we know its content, so we're creating
            // an invalid object.
            var serviceProvider = new ServiceProvider( null!, null );

            var servicesBuilder = this._services.ToBuilder();

            foreach ( var s in services )
            {
                AddService( new Lazy<IService, Type>( () => s.CreateFunc( serviceProvider ), s.Type ), servicesBuilder );
            }

            serviceProvider._services = servicesBuilder.ToImmutable();

            return serviceProvider;
        }

        /// <summary>
        /// Adds the services that have the same scope as the project processing itself.
        /// </summary>
        /// <param name="metadataReferences">A list of resolved metadata references for the current project.</param>
        public ServiceProvider WithProjectScopedServices( IEnumerable<MetadataReference> metadataReferences )
        {
            // ReflectionMapperFactory cannot be a global service because it keeps a reference from compilations to types of the
            // user assembly. When we need to unload the user assembly, we first need to unload the ReflectionMapperFactory.
            var serviceProvider = this.WithServices( new ReflectionMapperFactory(), new AssemblyLocator( metadataReferences ) );

            serviceProvider = serviceProvider.WithServices( new SyntaxSerializationService( serviceProvider ), new CompileTimeTypeFactory() );
            serviceProvider = serviceProvider.WithServices( new SystemTypeResolver( serviceProvider ) );
            serviceProvider = serviceProvider.WithService( new UserCodeInvoker( serviceProvider ) );

            return serviceProvider;
        }

        /// <summary>
        /// Adds the next service provider in a chain.
        /// </summary>
        /// <param name="nextProvider"></param>
        /// <remarks>
        /// When the current service provider fails to find a service, it will try to find it using the next provider in the chain.
        /// </remarks>
        public ServiceProvider WithNextProvider( IServiceProvider nextProvider ) => new ServiceProvider( this._services, nextProvider );

        public override string ToString()
        {
            var mark = this.GetOptionalService<ServiceProviderMark>();

            return $"ServiceProvider Mark='{mark}', Entries={this._services.Count}";
        }
    }
}