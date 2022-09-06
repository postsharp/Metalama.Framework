// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests
{
    public class ServiceProviderTests
    {
        [Fact]
        public async Task AsyncLocalInstanceAsync()
        {
            Assert.False( ServiceProviderFactory.HasAsyncLocalProvider );

            var backstageServiceProvider = new TestServiceProvider();

            ServiceProviderFactory.InitializeAsyncLocalProvider( backstageServiceProvider );
            Assert.True( ServiceProviderFactory.HasAsyncLocalProvider );

            Assert.Same( backstageServiceProvider, ServiceProviderFactory.GetServiceProvider().GetRequiredBackstageService<ITempFileManager>() );

            await Task.Yield();

            Assert.True( ServiceProviderFactory.HasAsyncLocalProvider );
            Assert.Same( backstageServiceProvider, ServiceProviderFactory.GetServiceProvider().GetRequiredBackstageService<ITempFileManager>() );

            ServiceProviderFactory.AddAsyncLocalService( new TestService() );

            _ = ServiceProviderFactory.GetServiceProvider().GetRequiredService<TestService>();

            ServiceProviderFactory.ResetAsyncLocalProvider();

            Assert.False( ServiceProviderFactory.HasAsyncLocalProvider );
        }

        private class TestService : IService { }

        private class TestServiceProvider : IServiceProvider, ITempFileManager
        {
            public object? GetService( Type serviceType )
            {
                if ( serviceType == typeof(ITempFileManager) )
                {
                    return this;
                }
                else
                {
                    return null;
                }
            }

            public string GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, Guid? guid = null ) => throw new NotImplementedException();
        }
    }
}