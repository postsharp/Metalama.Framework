// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public class ServiceProviderTests
    {
        [Fact]
        public async Task TestAsyncLocal()
        {
            Assert.False( ServiceProviderFactory.HasAsyncLocalProvider );

            TestProjectOptions myOptions = new();

            ServiceProviderFactory.InitializeAsyncLocalProvider( myOptions );
            Assert.True( ServiceProviderFactory.HasAsyncLocalProvider );

            Assert.Same( myOptions, ServiceProviderFactory.GetServiceProvider().GetService<IPathOptions>() );

            await Task.Yield();

            Assert.True( ServiceProviderFactory.HasAsyncLocalProvider );
            Assert.Same( myOptions, ServiceProviderFactory.GetServiceProvider().GetService<IPathOptions>() );

            ServiceProviderFactory.ResetAsyncLocalProvider();

            Assert.False( ServiceProviderFactory.HasAsyncLocalProvider );
        }
    }
}