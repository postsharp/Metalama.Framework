// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class DesignTimeEntryPointManagerTests
    {
        private readonly ImmutableDictionary<string, int> _contractVersion = ImmutableDictionary<string, int>.Empty.Add(
            "1.0",
            ContractsVersion.ContractVersion_1_0 );

        [Fact]
        public async Task RegisterBeforeGetAsync()
        {
            IDesignTimeEntryPointManager manager = new DesignTimeEntryPointManager();
            var consumer = manager.GetConsumer( this._contractVersion );
            var version = new Version( 1, 0 );
            var provider = new FakeProvider( version );
            manager.RegisterServiceProvider( provider );
            Assert.Equal( provider, await consumer.GetServiceProviderAsync( version, CancellationToken.None ) );
        }

        [Fact]
        public async Task GetBeforeRegisterAsync()
        {
            IDesignTimeEntryPointManager manager = new DesignTimeEntryPointManager();
            var consumer = manager.GetConsumer( this._contractVersion );
            var version = new Version( 1, 0 );
            var getTask = consumer.GetServiceProviderAsync( version, CancellationToken.None );
            Assert.False( getTask.IsCompleted );

            var provider = new FakeProvider( version );
            manager.RegisterServiceProvider( provider );
            Assert.Equal( provider, await getTask );
        }

        [Fact]
        public async Task CancelGetSAsync()
        {
            IDesignTimeEntryPointManager manager = new DesignTimeEntryPointManager();
            var consumer = manager.GetConsumer( this._contractVersion );
            var version = new Version( 1, 0 );
            CancellationTokenSource cancellationTokenSource = new();
            var task = consumer.GetServiceProviderAsync( version, cancellationTokenSource.Token );
            Assert.False( task.IsCompleted );
            cancellationTokenSource.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>( async () => await task );
        }

        [Fact]
        public void Version()
        {
            IDesignTimeEntryPointManager manager = new DesignTimeEntryPointManager();
            Assert.NotNull( manager.Version );
        }

        private class FakeProvider : ICompilerServiceProvider
        {
            public FakeProvider( Version version, int contractsVersion = 0 )
            {
                this.Version = version;
                this.ContractsVersion = contractsVersion;
            }

            public Version Version { get; }

            public ImmutableDictionary<string, int> ContractVersions
                => ImmutableDictionary<string, int>.Empty.Add( "1.0", Framework.DesignTime.Contracts.ContractsVersion.ContractVersion_1_0 );

            public T? GetService<T>()
                where T : class, ICompilerService
                => throw new NotImplementedException();
        }
    }
}