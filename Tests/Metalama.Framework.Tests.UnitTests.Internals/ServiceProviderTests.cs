// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Project;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests
{
    public class ServiceProviderTests
    {
        [Fact]
        public async Task AsyncLocalInstance()
        {
            Assert.False( ServiceProviderFactory.HasAsyncLocalProvider );

            TestProjectOptions myOptions = new();

            ServiceProviderFactory.InitializeAsyncLocalProvider( myOptions );
            Assert.True( ServiceProviderFactory.HasAsyncLocalProvider );

            Assert.Same( myOptions, ServiceProviderFactory.GetServiceProvider().GetService<IPathOptions>() );

            await Task.Yield();

            Assert.True( ServiceProviderFactory.HasAsyncLocalProvider );
            Assert.Same( myOptions, ServiceProviderFactory.GetServiceProvider().GetService<IPathOptions>() );

            ServiceProviderFactory.AddAsyncLocalService( new TestService() );

            _ = ServiceProviderFactory.GetServiceProvider().GetService<TestService>();

            ServiceProviderFactory.ResetAsyncLocalProvider();

            Assert.False( ServiceProviderFactory.HasAsyncLocalProvider );
        }

        private class TestService : IService { }
    }
}