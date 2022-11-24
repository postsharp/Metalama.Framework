// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests
{
    public class ServiceProviderTests : TestBase
    {
        // It may seem redundant to test both IGlobalService and IProjectService, but there was a bug where the interface name was hardcoded
        // instead of the generic parameter.

        [Fact]
        public void GlobalService_NextProvider()
        {
            var serviceProvider1 = ServiceProvider<IGlobalService>.Empty.WithService( new TestGlobalService() );
            Assert.NotNull( serviceProvider1.GetService<TestGlobalService>() );

            var serviceProvider2 = ServiceProvider<IGlobalService>.Empty.WithNextProvider( serviceProvider1 );
            Assert.NotNull( serviceProvider2.GetService<TestGlobalService>() );
        }

        [Fact]
        public void GlobalService_AccessByInterface()
        {
            var serviceProvider1 = ServiceProvider<IGlobalService>.Empty.WithService( new TestGlobalService() );
            Assert.NotNull( serviceProvider1.GetService<TestGlobalService>() );
            Assert.NotNull( serviceProvider1.GetService<ITestGlobalService>() );
        }

        [Fact]
        public void GlobalService_NextProvider_AccessByInterface()
        {
            var serviceProvider1 = ServiceProvider<IGlobalService>.Empty.WithService( new TestGlobalService() );
            Assert.NotNull( serviceProvider1.GetService<TestGlobalService>() );
            Assert.NotNull( serviceProvider1.GetService<ITestGlobalService>() );
            var serviceProvider2 = ServiceProvider<IGlobalService>.Empty.WithNextProvider( serviceProvider1 );
            Assert.NotNull( serviceProvider2.GetService<ITestGlobalService>() );
        }

        [Fact]
        public void ProjectService_NextProvider()
        {
            var serviceProvider1 = ServiceProvider<IProjectService>.Empty.WithService( new TestProjectService() );
            Assert.NotNull( serviceProvider1.GetService<TestProjectService>() );

            var serviceProvider2 = ServiceProvider<IProjectService>.Empty.WithNextProvider( serviceProvider1 );
            Assert.NotNull( serviceProvider2.GetService<TestProjectService>() );
        }

        [Fact]
        public void ProjectService_AccessByInterface()
        {
            var serviceProvider1 = ServiceProvider<IProjectService>.Empty.WithService( new TestProjectService() );
            Assert.NotNull( serviceProvider1.GetService<TestProjectService>() );
            Assert.NotNull( serviceProvider1.GetService<ITestProjectService>() );
        }

        [Fact]
        public void GlobalProject_NextProvider_AccessByInterface()
        {
            var serviceProvider1 = ServiceProvider<IProjectService>.Empty.WithService( new TestProjectService() );
            Assert.NotNull( serviceProvider1.GetService<TestProjectService>() );
            Assert.NotNull( serviceProvider1.GetService<ITestProjectService>() );
            var serviceProvider2 = ServiceProvider<IProjectService>.Empty.WithNextProvider( serviceProvider1 );
            Assert.NotNull( serviceProvider2.GetService<ITestProjectService>() );
        }

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

            ServiceProviderFactory.AddAsyncLocalService( new TestGlobalService() );

            _ = ServiceProviderFactory.GetServiceProvider().GetRequiredService<TestGlobalService>();

            ServiceProviderFactory.ResetAsyncLocalProvider();

            Assert.False( ServiceProviderFactory.HasAsyncLocalProvider );
        }

        [Fact]
        public void Observer()
        {
            var observer = new DifferObserver();
            var mocks = new TestServiceCollection( observer );
            using var testContext = this.CreateTestContext( mocks );
            Assert.Same( observer, testContext.ServiceProvider.Global.GetService<IDifferObserver>() );
        }

        private interface ITestGlobalService : IGlobalService { }

        private interface ITestProjectService : IProjectService { }

        private class TestGlobalService : ITestGlobalService { }

        private class TestProjectService : ITestProjectService { }

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