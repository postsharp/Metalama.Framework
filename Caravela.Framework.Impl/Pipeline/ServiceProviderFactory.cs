// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Serialization;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    public static class ServiceProviderFactory
    {
        private static ServiceProvider? _sharedInstance;

        private static ServiceProvider CreateBasicServices( IDirectoryOptions directoryOptions, bool freeze )
        {
            ServiceProvider serviceProvider = new();
            serviceProvider.AddService( directoryOptions );
            serviceProvider.AddService( new ReferenceAssemblyLocator( serviceProvider ) );
            serviceProvider.AddService( new SymbolClassificationService( serviceProvider ) );
            serviceProvider.AddService( new SyntaxSerializationService() );
            serviceProvider.AddService( new SystemTypeResolver( serviceProvider ) );

            if ( freeze )
            {
                serviceProvider.Freeze();
            }

            return serviceProvider;
        }

        public static ServiceProvider Shared
            => LazyInitializer.EnsureInitialized( ref _sharedInstance, () => CreateBasicServices( DefaultDirectoryOptions.Instance, true ) )!;

        public static ServiceProvider GetServiceProvider( IDirectoryOptions? directoryOptions = null, IAssemblyLocator? assemblyLocator = null )
        {
            ServiceProvider serviceProvider;

            if ( directoryOptions == null )
            {
                // If we are not given specific directories, we try to provide shared, singleton instances of the services that don't depend on
                // any other configuration. This avoids redundant initializations and improves performance.
                serviceProvider = new ServiceProvider( Shared );
            }
            else
            {
                serviceProvider = CreateBasicServices( directoryOptions, false );
            }

            if ( assemblyLocator != null )
            {
                serviceProvider.AddService( assemblyLocator );
            }

            return serviceProvider;
        }
    }
}