// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Project;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    public static class ServiceProviderFactory
    {
        private static readonly AsyncLocal<ServiceProvider?> _asyncLocalInstance = new();
        private static ServiceProvider? _globalInstance;

        /// <summary>
        /// Registers a global service, which will be available in the <see cref="GlobalProvider"/> provider and in all
        /// instances returned by <see cref="GetServiceProvider"/>. This method is used by TryMetalama to register hooks.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public static void AddGlobalService<T>( T service )
            where T : IService
        {
            _globalInstance = GlobalProvider.WithServices( service );
            _asyncLocalInstance.Value = null;
        }

        /// <summary>
        /// Replaces the async-local <see cref="ServiceProvider"/> by a newly created provider, with a new instances
        /// of all services. This method must be called when the consumer needs to pass a different implementation
        /// of <see cref="IServiceProvider"/> than the default one cannot call <see cref="GetServiceProvider"/>
        /// because it does not control the calling point. A typical consumer of this method is TryMetalama.
        /// </summary>
        public static void InitializeAsyncLocalProvider( IServiceProvider backstageServiceProvider )
        {
            _asyncLocalInstance.Value = CreateBaseServiceProvider( backstageServiceProvider )
                .WithMark( ServiceProviderMark.AsyncLocal );
        }

        public static bool HasAsyncLocalProvider => _asyncLocalInstance.Value != null;

        public static void ResetAsyncLocalProvider()
        {
            _asyncLocalInstance.Value = null;
        }

        /// <summary>
        /// Add a service to the async-local <see cref="ServiceProvider"/>, which is used as a prototype by the
        /// <see cref="GetServiceProvider"/> method to create instances in the current async context. If no async-local
        /// context is defined yet, it is cloned from <see cref="GlobalProvider"/>.
        /// </summary>
        public static void AddAsyncLocalService( IService service )
        {
            _asyncLocalInstance.Value = AsyncLocalProvider.WithServices( service );
        }

        private static ServiceProvider CreateBaseServiceProvider( IServiceProvider? nextServiceProvider )
        {
            var serviceProvider = ServiceProvider.Empty
                .WithNextProvider( nextServiceProvider ?? BackstageServiceFactory.ServiceProvider );

            serviceProvider = serviceProvider
                .WithServices( new DefaultCompileTimeDomainFactory() )
                .WithSharedLazyInitializedService( sp => new ReferenceAssemblyLocator( sp ) );

            return serviceProvider;
        }

        /// <summary>
        /// Gets the default <see cref="ServiceProvider"/> instance.
        /// </summary>
        public static ServiceProvider GlobalProvider
            => LazyInitializer.EnsureInitialized(
                ref _globalInstance,
                () => CreateBaseServiceProvider( null ).WithMark( ServiceProviderMark.Global ) )!;

        internal static ServiceProvider AsyncLocalProvider => _asyncLocalInstance.Value ??= GlobalProvider;

        /// <summary>
        /// Gets a new instance of the <see cref="ServiceProvider"/>. If a fallback <see cref="IServiceProvider"/> is provided,
        /// the new <see cref="ServiceProvider"/> gets a new implementation of all shared service (i.e. <see cref="AddGlobalService{T}"/> and
        /// <see cref="AddAsyncLocalService"/> are ignored). This scenario is used in tests. Otherwise, a shallow clone of the async-local or the global
        /// provider is provided.
        /// </summary>
        public static ServiceProvider GetServiceProvider( IServiceProvider? nextServiceProvider = null )
        {
            ServiceProvider serviceProvider;

            if ( nextServiceProvider == null )
            {
                // If we are not given specific directories, we try to provide shared, singleton instances of the services that don't depend on
                // any other configuration. This avoids redundant initializations and improves performance.
                serviceProvider = AsyncLocalProvider;
            }
            else
            {
                serviceProvider = CreateBaseServiceProvider( nextServiceProvider );
            }

            return serviceProvider;
        }
    }
}