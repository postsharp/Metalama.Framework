// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if DEBUG // These tests are debug-only because TestableCancellationToken is testable in the DEBUG config only.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.DesignTime.VisualStudio;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities.Threading;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

public class SourceGeneratorIntegrationTests : LoggingTestBase
{
    private const int _maxCancellationPoints = 22;

    public SourceGeneratorIntegrationTests( ITestOutputHelper logger ) : base( logger ) { }

    [Theory]
    [ClassData( typeof(GetCancellationPoints) )]
    public async Task WithCancellation( int cancelOnCancellationPointIndex )
    {
        var wasCancellationRequested = await this.RunTestAsync( cancelOnCancellationPointIndex );
        Assert.True( wasCancellationRequested );
    }

    [Fact]
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

    [Fact]
    public async Task WithoutCancellation()
    {
        var wasCancellationRequested = await this.RunTestAsync( int.MaxValue );
        Assert.False( wasCancellationRequested );
    }

    private async Task<bool> RunTestAsync( int cancelOnCancellationPointIndex ) // Return value: whether the test was cancelled.
    {
        using var testContext = this.CreateTestContext( new TestProjectOptions( hasSourceGeneratorTouchFile: true ) );
        var serviceProvider = testContext.ServiceProvider;

        var projectKey = ProjectKey.CreateTest( "project" );

        // Start the hub service on both ends.
        var hubPipeName = $"Metalama_Hub_{Guid.NewGuid()}";

        using var userProcessServiceHubEndpoint = new UserProcessServiceHubEndpoint( serviceProvider, hubPipeName );
        userProcessServiceHubEndpoint.Start();
        using var analysisProcessServiceHubEndpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, hubPipeName );
        _ = analysisProcessServiceHubEndpoint.ConnectAsync(); // Do not await so we get more randomness.

        // Start the main services on both ends.
        using var analysisProcessEndpoint = new AnalysisProcessEndpoint(
            serviceProvider.WithService( analysisProcessServiceHubEndpoint ),
            hubPipeName );

        analysisProcessEndpoint.Start();

        var testCancellationTokenSourceFactory = new TestCancellationTokenSourceFactory( cancelOnCancellationPointIndex );
        var cancellationTokenSource = testCancellationTokenSourceFactory.Create();

        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var analysisProcessProjectHandlerObserver = new ProjectHandlerObserver();
        var analysisProcessServiceProvider = serviceProvider.WithServices( analysisProcessEndpoint, pipelineFactory, analysisProcessProjectHandlerObserver );
        using var analysisProcessProjectHandler = new VsAnalysisProcessProjectHandler( analysisProcessServiceProvider, testContext.ProjectOptions, projectKey );

        var userProcessProjectHandlerObserver = new ProjectHandlerObserver();
        var userProcessServiceProvider = serviceProvider.WithServices( userProcessServiceHubEndpoint, userProcessProjectHandlerObserver );

        using var userProcessProjectHandler = new VsUserProcessProjectHandler( userProcessServiceProvider, testContext.ProjectOptions, projectKey );

        // Awaiting here avoids cancellations in the middle of the remoting initialization.
        await analysisProcessEndpoint.WaitUntilInitializedAsync();
        await analysisProcessProjectHandler.PendingTasks.WaitAllAsync();
        await userProcessProjectHandler.PendingTasks.WaitAllAsync();

        // The first run of the pipeline is synchronous.
        var wasCancellationRequested = ExecutePipeline( 1 );

        // The second run of the pipeline is asynchronous so it uses a different execution path.
        wasCancellationRequested |= ExecutePipeline( 2 );

        // For the third run should go the same path as the second run, but the second run may have been cancelled.
        // For the third run, we take cancellation into account.
        testCancellationTokenSourceFactory.StopCounting();
        wasCancellationRequested |= ExecutePipeline( 3 );

        bool ExecutePipeline( int version )
        {
            try
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

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
                    Assert.Contains( $"__M{version}", analysisProcessGenerateSources.AdditionalSources.First().Value.GeneratedSyntaxTree.ToString() );
                }

                var asynchronouslyPublishedSource = analysisProcessProjectHandlerObserver.PublishedSources.Take( cancellationTokenSource.Token );
                Assert.Single( asynchronouslyPublishedSource );
                Assert.Contains( $"__M{version}", asynchronouslyPublishedSource.First().Value );

                // Publishing is never synchronous.
                var userProcessGeneratedSources = userProcessProjectHandlerObserver.PublishedSources.Take( cancellationTokenSource.Token );

                Assert.Single( userProcessGeneratedSources );
                Assert.Contains( $"__M{version}", userProcessGeneratedSources.First().Value );

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

        return wasCancellationRequested;
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

        public override void Dispose()
        {
            // Do not dispose because the instance is shared.
        }
    }

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

    private class ProjectHandlerObserver : IProjectHandlerObserver
    {
        public BlockingCollection<ImmutableDictionary<string, string>> PublishedSources { get; } = new();

        public void OnGeneratedCodePublished( ProjectKey projectKey, ImmutableDictionary<string, string> sources )
        {
            this.PublishedSources.Add( sources );
        }
    }
}
#endif