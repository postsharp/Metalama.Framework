// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Utilities;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    public static class ServiceProviderFactory
    {
        private static readonly AsyncLocal<ServiceProvider?> _asyncLocalInstance = new();
        private static ServiceProvider? _globalInstance;

        /// <summary>
        /// Registers a global service, which will be available in the <see cref="GlobalProvider"/> provider and in all
        /// instances returned by <see cref="GetServiceProvider"/>. This method is used by TryCaravela to register hooks.
        /// </summary>
        public static void AddGlobalService<T>( T service )
            where T : IService
        {
            var newServices = new ServiceProvider( GlobalProvider );
            newServices.AddService( service );
            newServices.Freeze();

            _globalInstance = newServices;

            _asyncLocalInstance.Value = null;
        }

        /// <summary>
        /// Replaces the async-local <see cref="ServiceProvider"/> by a newly created provider, with a new instances
        /// of all services. This method must be called when the consumer needs to pass a different implementation
        /// of <see cref="IDirectoryOptions"/> than the default one cannot call <see cref="GetServiceProvider"/>
        /// because it does not control the calling point. A typical consumer of this method is TryCaravela.
        /// </summary>
        public static void InitializeAsyncLocalProvider( IDirectoryOptions? directoryOptions = null )
        {
            _asyncLocalInstance.Value = CreateBaseServiceProvider( directoryOptions ?? DefaultDirectoryOptions.Instance, true );
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
            var newServices = new ServiceProvider( AsyncLocalProvider );
            newServices.AddService( service );
            newServices.Freeze();

            _asyncLocalInstance.Value = newServices;
        }

        private static ServiceProvider CreateBaseServiceProvider( IDirectoryOptions directoryOptions, bool freeze )
        {
            ServiceProvider serviceProvider = new();
            serviceProvider.AddService( directoryOptions );
            serviceProvider.AddService( new ReferenceAssemblyLocator( serviceProvider ) );
            serviceProvider.AddService( new SymbolClassificationService( serviceProvider ) );
            serviceProvider.AddService( new SyntaxSerializationService() );
            serviceProvider.AddService( new SystemTypeResolver( serviceProvider ) );
            serviceProvider.AddService( new DefaultCompileTimeDomainFactory() );
            serviceProvider.AddService( new CompileTimeExceptionHandler() );

            if ( freeze )
            {
                serviceProvider.Freeze();
            }

            return serviceProvider;
        }

        /// <summary>
        /// Gets the default <see cref="ServiceProvider"/> instance.
        /// </summary>
        public static ServiceProvider GlobalProvider
            => LazyInitializer.EnsureInitialized( ref _globalInstance, () => CreateBaseServiceProvider( DefaultDirectoryOptions.Instance, true ) )!;

        internal static ServiceProvider AsyncLocalProvider => _asyncLocalInstance.Value ??= GlobalProvider;

        /// <summary>
        /// Gets a new instance of the <see cref="ServiceProvider"/>. If an implementation of <see cref="IDirectoryOptions"/> is provided,
        /// the new <see cref="ServiceProvider"/> gets a new implementation of all shared service (i.e. <see cref="AddGlobalService{T}"/> and
        /// <see cref="AddAsyncLocalService"/> are ignored). This scenario is used in tests. Otherwise, a shallow clone of the async-local or the global
        /// provider is provided.
        /// </summary>
        public static ServiceProvider GetServiceProvider( IDirectoryOptions? directoryOptions = null, IAssemblyLocator? assemblyLocator = null )
        {
            ServiceProvider serviceProvider;

            if ( directoryOptions == null )
            {
                // If we are not given specific directories, we try to provide shared, singleton instances of the services that don't depend on
                // any other configuration. This avoids redundant initializations and improves performance.
                serviceProvider = new ServiceProvider( AsyncLocalProvider );
            }
            else
            {
                serviceProvider = CreateBaseServiceProvider( directoryOptions, false );
            }

            if ( assemblyLocator != null )
            {
                serviceProvider.AddService( assemblyLocator );
            }

            // This service must be added last because it depends may depend on a service added previously.
            // (This is not nice but this is the only case).
            serviceProvider.AddService( new UserCodeInvoker( serviceProvider ) );

            return serviceProvider;
        }
    }
}