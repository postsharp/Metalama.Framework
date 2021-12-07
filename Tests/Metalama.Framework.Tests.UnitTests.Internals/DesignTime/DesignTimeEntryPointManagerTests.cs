// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class DesignTimeEntryPointManagerTests
    {
        [Fact]
        public async Task RegisterBeforeGet()
        {
            IDesignTimeEntryPointManager instance = new DesignTimeEntryPointManager();
            var version = new Version( 1, 0 );
            var provider = new FakeProvider( version );
            instance.RegisterServiceProvider( provider );
            Assert.Equal( provider, await instance.GetServiceProviderAsync( version, CancellationToken.None ) );
        }

        [Fact]
        public async Task GetBeforeRegister()
        {
            IDesignTimeEntryPointManager instance = new DesignTimeEntryPointManager();
            var version = new Version( 1, 0 );
            var getTask = instance.GetServiceProviderAsync( version, CancellationToken.None );
            Assert.False( getTask.IsCompleted );

            var provider = new FakeProvider( version );
            instance.RegisterServiceProvider( provider );
            Assert.Equal( provider, await getTask );
        }

        [Fact]
        public async Task CancelGetS()
        {
            IDesignTimeEntryPointManager instance = new DesignTimeEntryPointManager();
            var version = new Version( 1, 0 );
            CancellationTokenSource cancellationTokenSource = new();
            var task = instance.GetServiceProviderAsync( version, cancellationTokenSource.Token );
            Assert.False( task.IsCompleted );
            cancellationTokenSource.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>( async () => await task );
        }

        [Fact]
        public void Version()
        {
            IDesignTimeEntryPointManager instance = new DesignTimeEntryPointManager();
            Assert.NotNull( instance.Version );
        }

        [Fact]
        public void Unload()
        {
            IDesignTimeEntryPointManager instance = new DesignTimeEntryPointManager();
            var version = new Version( 1, 0 );
            var provider = new FakeProvider( version );
            instance.RegisterServiceProvider( provider );
            var task = instance.GetServiceProviderAsync( version, CancellationToken.None );
            Assert.True( task.IsCompleted );
            Assert.Equal( provider, task.Result );
            provider.Unload();
            var task2 = instance.GetServiceProviderAsync( version, CancellationToken.None );
            Assert.False( task2.IsCompleted );
        }

        private class FakeProvider : ICompilerServiceProvider
        {
            public FakeProvider( Version version )
            {
                this.Version = version;
            }

            public Version Version { get; }

            public ICompilerService? GetCompilerService( Type type ) => throw new NotImplementedException();

            public event Action? Unloaded;

            public void Unload() => this.Unloaded?.Invoke();
        }
    }
}