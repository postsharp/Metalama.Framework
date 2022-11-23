// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Engine.Metrics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Services
{
    public static class ServiceProviderFactory
    {
        private static readonly AsyncLocal<ServiceProvider<IGlobalService>?> _asyncLocalInstance = new();
        private static ServiceProvider<IGlobalService>? _globalInstance;

        static ServiceProviderFactory()
        {
            ModuleInitializer.EnsureInitialized();
        }

        /// <summary>
        /// Replaces the async-local <see cref="ServiceProvider{TBase}"/> by a newly created provider, with a new instances
        /// of all services. This method must be called when the consumer needs to pass a different implementation
        /// of <see cref="IServiceProvider"/> than the default one cannot call <see cref="GetServiceProvider()"/>
        /// because it does not control the calling point. A typical consumer of this method is TryMetalama.
        /// </summary>
        public static void InitializeAsyncLocalProvider( IServiceProvider backstageServiceProvider )
        {
            _asyncLocalInstance.Value = CreateBaseServiceProvider( backstageServiceProvider );
        }

        public static bool HasAsyncLocalProvider => _asyncLocalInstance.Value != null;

        public static void ResetAsyncLocalProvider()
        {
            _asyncLocalInstance.Value = null;
        }

        /// <summary>
        /// Add a service to the async-local <see cref="ServiceProvider{TBase}"/>, which is used as a prototype by the
        /// <see cref="GetServiceProvider()"/> method to create instances in the current async context. If no async-local
        /// context is defined yet, it is cloned from <see cref="GlobalProvider"/>.
        /// </summary>
        public static void AddAsyncLocalService( IGlobalService service )
        {
            _asyncLocalInstance.Value = AsyncLocalProvider.WithServices( service );
        }
        
        
        private static ServiceProvider<IGlobalService> CreateBaseServiceProvider(
            IServiceProvider? nextServiceProvider,
            MocksFactory? mocks = null )
        {
            var serviceProvider = ServiceProvider<IGlobalService>.Empty
                .WithNextProvider( nextServiceProvider ?? BackstageServiceFactory.ServiceProvider );
            
            if ( mocks != null )
            {
                // We hook both the mocked services and the MockFactory itself, so that other levels of factory method
                // know about them.
                serviceProvider = mocks.GlobalServices.ServiceProvider.WithNextProvider( serviceProvider ).WithService( mocks );
            }

            serviceProvider = serviceProvider
                .TryWithService<ITestableCancellationTokenSourceFactory>( _ => new DefaultTestableCancellationTokenSource() )
                .TryWithService<ICompileTimeDomainFactory>( _ => new DefaultCompileTimeDomainFactory() )
                .TryWithService<IMetalamaProjectClassifier>( _ => new MetalamaProjectClassifier() )
                .TryWithService( sp => new UserCodeInvoker( sp ) )
                .TryWithService( _ => new ReferenceAssemblyLocatorProvider() )
                .TryWithService<ISystemTypeResolverFactory>( _ => new SystemTypeResolverFactory() );

           

            return serviceProvider;
        }

        /// <summary>
        /// Gets the default <see cref="ServiceProvider{TBase}"/> instance.
        /// </summary>
        public static ServiceProvider<IGlobalService> GlobalProvider
            => LazyInitializer.EnsureInitialized(
                ref _globalInstance,
                () => CreateBaseServiceProvider( null ) );

        internal static ServiceProvider<IGlobalService> AsyncLocalProvider => _asyncLocalInstance.Value ??= GlobalProvider;

        /// <summary>
        /// Gets an instance of the <see cref="ServiceProvider{TBase}"/> backed with the default backstage services.
        /// </summary>
        public static ServiceProvider<IGlobalService> GetServiceProvider() => GetServiceProvider( null );

        /// <summary>
        /// Gets an instance of <see cref="ServiceProvider{TBase}"/> with a specific upstream <see cref="IServiceProvider"/>.
        /// If <see cref="AddAsyncLocalService"/> has been called, the <paramref name="upstreamServiceProvider"/> parameter is ignored.
        /// This situation happens in Metalama.Try.
        /// </summary>
        public static ServiceProvider<IGlobalService> GetServiceProvider(
            IServiceProvider? upstreamServiceProvider,
            MocksFactory? mocks = null )
        {
            ServiceProvider<IGlobalService> serviceProvider;

            if ( _asyncLocalInstance.Value != null )
            {
                // If we are not given specific directories, we try to provide shared, singleton instances of the services that don't depend on
                // any other configuration. This avoids redundant initializations and improves performance.
                serviceProvider = AsyncLocalProvider;
            }
            else
            {
                serviceProvider = CreateBaseServiceProvider( upstreamServiceProvider, mocks );
            }

            return serviceProvider;
        }

        public static ServiceProvider<IProjectService> WithProjectScopedServices(
            this IServiceProvider<IGlobalService> serviceProvider,
            IProjectOptions projectOptions,
            Compilation compilation )
            => serviceProvider.WithProjectScopedServices( projectOptions, compilation.References );

        /// <summary>
        /// Adds the services that have the same scope as the project processing itself.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="projectOptions"></param>
        /// <param name="metadataReferences">A list of resolved metadata references for the current project.</param>
        public static ServiceProvider<IProjectService> WithProjectScopedServices(
            this IServiceProvider<IGlobalService> serviceProvider,
            IProjectOptions projectOptions,
            IEnumerable<MetadataReference> metadataReferences )
        {
            var projectServiceProvider = ServiceProvider<IProjectService>.Empty.WithNextProvider( serviceProvider ).WithService( projectOptions );

            var mocks = serviceProvider.GetService<MocksFactory>();
            
            if ( mocks != null )
            {
                projectServiceProvider = mocks.ProjectServices.ServiceProvider.WithNextProvider( projectServiceProvider );
            }

            if ( projectServiceProvider.GetService<ITaskScheduler>() == null )
            {
                // We use a single-threaded task scheduler for tests because the test runner itself is already multi-threaded and
                // most tests are so small that they do not allow for significant concurrency anyway. A specific test can provide a different scheduler.
                // We randomize the ordering of execution to improve the test relevance.
                ITaskScheduler taskScheduler;
                
                if ( projectOptions.IsTest )
                {
                    taskScheduler = new RandomizingSingleThreadedTaskScheduler( serviceProvider );
                }
                else
                {
                    taskScheduler = projectOptions.IsConcurrentBuildEnabled ? new ConcurrentTaskScheduler() : new SingleThreadedTaskScheduler();
                }

                projectServiceProvider = projectServiceProvider.WithService( taskScheduler );
            }

            projectServiceProvider = projectServiceProvider
                .TryWithService<SerializerFactoryProvider>( sp => new BuiltInSerializerFactoryProvider( sp ) )
                .TryWithService<IAssemblyLocator>( sp => new AssemblyLocator( sp, metadataReferences ) )
                .TryWithService( _ => new SyntaxSerializationService() )
                .TryWithService( sp => new CompilationContextFactory( sp ) );


            projectServiceProvider = projectServiceProvider.WithMetricProviders();

       

            return projectServiceProvider;
        }
    }

}