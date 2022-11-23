// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if DEBUG // These tests are debug-only because TestableCancellationToken is testable in the DEBUG config only.

using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.DesignTime.VisualStudio;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities.Threading;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

public class SourceGeneratorIntegrationTests : LoggingTestBase
{
    private const int _maxCancellationPoints = 23;

    public SourceGeneratorIntegrationTests( ITestOutputHelper logger ) : base( logger ) { }

    [Theory]
    [ClassData( typeof(GetCancellationPoints) )]
    public async Task WithCancellation( int cancelOnCancellationPointIndex )
    {
        var wasCancellationRequested = await this.RunTestAsync( cancelOnCancellationPointIndex );
        Assert.True( wasCancellationRequested );
    }

    [Fact( Skip = "32185" )]
    public async Task MaxCancellationPointsIsCorrect()
    {
        var isSmallEnough = await this.RunTestAsync( _maxCancellationPoints );
        var isLargeEnough = !await this.RunTestAsync( _maxCancellationPoints + 1 );

        if ( isSmallEnough && isLargeEnough )
        {
            // Successful.
            return;
        }

        var min = isSmallEnough ? _maxCancellationPoints : 1;
        var max = isLargeEnough ? _maxCancellationPoints : _maxCancellationPoints * 2;

        for ( var i = min; i < max; i++ )
        {
            if ( !await this.RunTestAsync( i ) )
            {
                this.Logger.WriteLine( $"The correct value for {nameof(_maxCancellationPoints)} is {i - 1}." );
                Assert.Equal( i - 1, _maxCancellationPoints );
            }
        }

        // Not enough iterations.
        Assert.False( true, "Cancellation was not requested. The value of the 'max' variable may be too low." );
    }

    [Fact( Skip = "32185" )]
    public async Task WithoutCancellation()
    {
        var wasCancellationRequested = await this.RunTestAsync( int.MaxValue );
        Assert.False( wasCancellationRequested );
    }

    private async Task<bool> RunTestAsync( int cancelOnCancellationPointIndex ) // Return value: whether the test was cancelled.
    {
        using var testContext = this.CreateTestContext( new TestProjectOptions( hasSourceGeneratorTouchFile: true ) );
        var serviceProvider = testContext.ServiceProvider.Global;
        serviceProvider = serviceProvider.WithService( new AnalysisProcessEventHub( serviceProvider ) );

        var projectKey = ProjectKeyFactory.CreateTest( "project" );

        // Start the hub service on both ends.
        var testGuid = Guid.NewGuid();
        var hubPipeName = $"Metalama_Hub_{testGuid}";
        var servicePipeName = $"Metalama_Analysis_{testGuid}";

        using var userProcessServiceHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, hubPipeName );
        userProcessServiceHubEndpoint.Start();
        using var analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, hubPipeName );
        _ = analysisProcessServiceHubEndpoint.ConnectAsync(); // Do not await so we get more randomness.

        // Start the main services on both ends.
        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            serviceProvider.WithService( analysisProcessServiceHubEndpoint ),
            servicePipeName );

        analysisProcessEndpoint.Start();

        var testCancellationTokenSourceFactory = new TestCancellationTokenSourceFactory( cancelOnCancellationPointIndex );
        var cancellationTokenSource = testCancellationTokenSourceFactory.Create();

        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var analysisProcessProjectHandlerObserver = new ProjectHandlerObserver();

        var analysisProcessServiceProvider = serviceProvider.WithServices(
            analysisProcessEndpoint,
            pipelineFactory,
            analysisProcessProjectHandlerObserver );

        using var analysisProcessProjectHandler =
            new VsAnalysisProcessProjectHandler( analysisProcessServiceProvider, testContext.ProjectOptions, projectKey );

        var userProcessProjectHandlerObserver = new ProjectHandlerObserver();
        var userProcessServiceProvider = serviceProvider.WithServices( userProcessServiceHubEndpoint, userProcessProjectHandlerObserver );

        using var userProcessProjectHandler = new VsUserProcessProjectHandler( userProcessServiceProvider, testContext.ProjectOptions, projectKey );

        bool wasCancellationRequested;

        try
        {
            // The first run of the pipeline is synchronous.
            wasCancellationRequested = ExecutePipeline( 1 );

            // The second run of the pipeline is asynchronous so it uses a different execution path.
            wasCancellationRequested |= ExecutePipeline( 2 );

            // For the third run should go the same path as the second run, but the second run may have been cancelled.
            // For the third run, we take cancellation into account.
            testCancellationTokenSourceFactory.StopCounting();
            wasCancellationRequested |= ExecutePipeline( 3 );
        }
        finally
        {
            // Awaiting here avoids cancellations in the middle of the remoting initialization.
            await analysisProcessEndpoint.WaitUntilInitializedAsync( "test" );
            await analysisProcessProjectHandler.PendingTasks.WaitAllAsync();
            await userProcessProjectHandler.PendingTasks.WaitAllAsync();
        }

        return wasCancellationRequested;

        string ReadTouchFile()
        {
            var touchFile = testContext.ProjectOptions.SourceGeneratorTouchFile.AssertNotNull();

            this.Logger.WriteLine( $"Reading the touch file '{touchFile}'." );

            return File.Exists( touchFile )
                ? RetryHelper.Retry( () => File.ReadAllText( touchFile ) )
                : "";
        }

        bool ExecutePipeline( int version )
        {
            // Touch files are updated during the initialization phase so it's good to clear the queues here.
            userProcessProjectHandlerObserver.Reset();
            analysisProcessProjectHandlerObserver.Reset();

            try
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                this.Logger.WriteLine( $"----- {version}-th execution of ExecutePipeline ---- " );

                var touchFileBefore = ReadTouchFile();

                // The code is an aspect that produces some introduction that depends on the target code,
                // then we change the target code in each version. If we change the aspect in each version, we 
                // will have to cope with pausing and resuming the pipeline.
                var aspectCode = @"
                using Metalama.Framework.Aspects;
                using Metalama.Framework.Code;
                using Metalama.Framework.Diagnostics;
                using Metalama.Framework.Eligibility;

                class MyAspect : TypeAspect
                {
                    public override void BuildAspect( IAspectBuilder<INamedType> builder )
                    {
                        foreach ( var m in builder.Target.Methods )
                        {
                            builder.Advice.IntroduceField( builder.Target, ""__"" + m.Name, typeof(int) );
                        }
                    }

                }
                ";

                var targetCode = $"[MyAspect] partial class C {{ void M{version}(){{}} }}";

                var compilation = CreateCSharpCompilation(
                    new Dictionary<string, string>() { ["aspect.cs"] = aspectCode, ["target.cs"] = targetCode },
                    name: projectKey.AssemblyName );

                var analysisProcessGenerateSources =
                    (SyntaxTreeSourceGeneratorResult) analysisProcessProjectHandler.GenerateSources( compilation, cancellationTokenSource.Token );

                Assert.Single( analysisProcessGenerateSources.AdditionalSources );

                if ( version == 1 )
                {
                    // For the first version, the code is published synchronously.
                    Assert.Contains(
                        $"__M{version}",
                        analysisProcessGenerateSources.AdditionalSources.First().Value.GeneratedSyntaxTree.ToString(),
                        StringComparison.Ordinal );
                }

                var asynchronouslyPublishedSource = analysisProcessProjectHandlerObserver.PublishedSources.Take( cancellationTokenSource.Token );
                Assert.Single( asynchronouslyPublishedSource );
                Assert.Contains( $"__M{version}", asynchronouslyPublishedSource.First().Value, StringComparison.Ordinal );

                // Publishing is never synchronous.
                var userProcessGeneratedSources = userProcessProjectHandlerObserver.PublishedSources.Take( cancellationTokenSource.Token );

                Assert.Single( userProcessGeneratedSources );
                Assert.Contains( $"__M{version}", userProcessGeneratedSources.First().Value, StringComparison.Ordinal );

                // Verify the touch file. First wait until it has been written, because it must be written after the generated code has been published.
                // We are not reading the file from the filesystem because it seems there may be some race situation where we get the old
                // content even if the new one was written.
                this.Logger.WriteLine( "Waiting for the touch file to be touched." );
                var touchFileAfter = analysisProcessProjectHandlerObserver.PublishedTouchFiles.Take();

                Assert.NotEqual( touchFileBefore, touchFileAfter );

                return false;
            }
            catch ( OperationCanceledException )
            {
                return true;
            }
            catch ( AggregateException aggregateException ) when ( aggregateException.InnerExceptions.Count == 1
                                                                   && aggregateException.InnerExceptions[0] is OperationCanceledException )
            {
                return true;
            }
        }
    }

    private class GetCancellationPoints : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator() => Enumerable.Range( 1, _maxCancellationPoints ).Select( i => new object[] { i } ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    private class TestCancellationTokenSource : TestableCancellationTokenSource
    {
        private readonly TestCancellationTokenSourceFactory _factory;

        public TestCancellationTokenSource( TestCancellationTokenSourceFactory factory )
        {
            this._factory = factory;
        }

        public override void OnPossibleCancellationPoint()
        {
            if ( this._factory.OnPossibleCancellationPoint() )
            {
                this.CancellationTokenSource.Cancel();
            }
        }

#pragma warning disable CA2215
        public override void Dispose()
        {
            // Do not dispose because the instance is shared.
        }
#pragma warning restore CA2215
    }

#pragma warning disable CA1001
    private class TestCancellationTokenSourceFactory : ITestableCancellationTokenSourceFactory
    {
        // We use a single source for testing because anyway we need to cancel all sources at the same time,
        // so it is equivalent to have one source. If we did not synchronize cancellations, we may cancel one source
        // but not the one we use for the blocking Take() method, and we would block forever.
        private readonly TestCancellationTokenSource _source;
        private readonly int _cancelOnCount;

        private int _count;
        private bool _isCounting = true;

        public TestCancellationTokenSourceFactory( int cancelOnCount )
        {
            this._cancelOnCount = cancelOnCount;
            this._source = new TestCancellationTokenSource( this );
        }

        public TestableCancellationTokenSource Create() => this._isCounting ? this._source : new TestableCancellationTokenSource();

        public bool OnPossibleCancellationPoint()
        {
            if ( this._isCounting )
            {
                if ( Interlocked.Increment( ref this._count ) == this._cancelOnCount )
                {
                    return true;
                }
            }

            return false;
        }

        public void StopCounting() => this._isCounting = false;
    }
#pragma warning restore CA1001

    private class ProjectHandlerObserver : IProjectHandlerObserver
    {
        public BlockingCollection<ImmutableDictionary<string, string>> PublishedSources { get; } = new();

        public BlockingCollection<string> PublishedTouchFiles { get; } = new();

        public void OnGeneratedCodePublished( ImmutableDictionary<string, string> sources )
        {
            this.PublishedSources.Add( sources );
        }

        public void OnTouchFileWritten( string content )
        {
            this.PublishedTouchFiles.Add( content );
        }

        public void Reset()
        {
            while ( this.PublishedTouchFiles.TryTake( out _ ) ) { }

            while ( this.PublishedSources.TryTake( out _ ) ) { }
        }
    }
}
#endif