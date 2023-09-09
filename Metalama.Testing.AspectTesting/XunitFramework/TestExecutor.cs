// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Testing.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.Testing.AspectTesting.XunitFramework
{
    internal sealed class TestExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
    {
        private readonly TestFactory _factory;
        private static readonly object _launchingDebuggerLock = new();
        private readonly GlobalServiceProvider _serviceProvider;
        private readonly ITaskRunner _taskRunner;
        private readonly ITestAssemblyMetadataReader _metadataReader;

        public TestExecutor( GlobalServiceProvider serviceProvider, AssemblyName assemblyName )
        {
            var assembly = Assembly.Load( assemblyName );
            var assemblyInfo = new ReflectionAssemblyInfo( assembly );
            TestDiscoverer discoverer = new( serviceProvider, assemblyInfo );
            var projectProperties = discoverer.GetTestProjectProperties();

            this._factory = new TestFactory(
                serviceProvider,
                projectProperties,
                new TestDirectoryOptionsReader( serviceProvider, projectProperties.ProjectDirectory ),
                assemblyInfo );

            this._serviceProvider = serviceProvider;
            this._taskRunner = this._serviceProvider.GetRequiredService<ITaskRunner>();
            this._metadataReader = this._serviceProvider.GetRequiredService<ITestAssemblyMetadataReader>();
        }

        public TestExecutor( GlobalServiceProvider serviceProvider, TestFactory factory )
        {
            this._factory = factory;
            this._serviceProvider = serviceProvider;
            this._taskRunner = this._serviceProvider.GetRequiredService<ITaskRunner>();
            this._metadataReader = this._serviceProvider.GetRequiredService<ITestAssemblyMetadataReader>();
        }

        void IDisposable.Dispose() { }

        public ITestCase Deserialize( string value )
        {
            return new TestCase( this._factory, value );
        }

        void ITestFrameworkExecutor.RunAll(
            IMessageSink executionMessageSink,
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestFrameworkExecutionOptions executionOptions )
        {
            throw new NotImplementedException();
        }

        public void RunTests(
            IEnumerable<ITestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions )
        {
            var hasLaunchedDebugger = false;
            var directoryOptionsReader = new TestDirectoryOptionsReader( this._serviceProvider, this._factory.ProjectProperties.ProjectDirectory );

            var collections = testCases.GroupBy( t => t.TestMethod.TestClass.TestCollection );

            var tasks = new ConcurrentDictionary<Task, Task>();
            var semaphore = new SemaphoreSlim( executionOptions.MaxParallelThreadsOrDefault() );
            var eventLock = new object();

            foreach ( var collection in collections )
            {
                var collectionMetrics = new Metrics( eventLock );

                collectionMetrics.Started += () =>
                {
                    executionMessageSink.OnMessage( new TestCollectionStarting( collection, collection.Key ) );

                    executionMessageSink.OnMessage(
                        new TestAssemblyStarting(
                            collection,
                            collection.Key.TestAssembly,
                            DateTime.Now,
                            "CompileTime",
                            "CompileTime" ) );
                };

                collectionMetrics.Finished += () =>
                {
                    executionMessageSink.OnMessage(
                        new TestAssemblyFinished(
                            collection,
                            collection.Key.TestAssembly,
                            collectionMetrics.ExecutionTime,
                            collectionMetrics.TestsRun,
                            collectionMetrics.TestFailed,
                            collectionMetrics.TestSkipped ) );

                    executionMessageSink.OnMessage(
                        new TestCollectionFinished(
                            collection,
                            collection.Key,
                            collectionMetrics.ExecutionTime,
                            collectionMetrics.TestsRun,
                            collectionMetrics.TestFailed,
                            collectionMetrics.TestSkipped ) );
                };

                var projectMetadata = this._metadataReader.GetMetadata( collection.Key.TestAssembly.Assembly );

                lock ( _launchingDebuggerLock )
                {
                    if ( projectMetadata.MustLaunchDebugger && !hasLaunchedDebugger )
                    {
                        Debugger.Launch();
                        hasLaunchedDebugger = true;
                    }
                }

                var projectReferences = projectMetadata.ToProjectReferences();

                foreach ( var type in collection.GroupBy( c => c.TestMethod.TestClass ) )
                {
                    var typeMetrics = new Metrics( collectionMetrics );
                    typeMetrics.Started += () => executionMessageSink.OnMessage( new TestClassStarting( type, type.Key ) );

                    typeMetrics.Finished += () => executionMessageSink.OnMessage(
                        new TestClassFinished(
                            type,
                            type.Key,
                            typeMetrics.ExecutionTime,
                            typeMetrics.TestsRun,
                            typeMetrics.TestFailed,
                            typeMetrics.TestSkipped ) );

                    typeMetrics.OnTestsDiscovered( type.Count() );

                    foreach ( var testCase in type )
                    {
                        var testMetrics = new Metrics( typeMetrics );
                        var test = new Test( testCase );
                        var logger = new TestOutputHelper( executionMessageSink, test );

                        testMetrics.Started += () =>
                        {
                            executionMessageSink.OnMessage( new TestMethodStarting( new[] { testCase }, testCase.TestMethod ) );
                            executionMessageSink.OnMessage( new TestCaseStarting( testCase ) );
                            executionMessageSink.OnMessage( new TestStarting( test ) );
                        };

                        testMetrics.Finished += () =>
                        {
                            executionMessageSink.OnMessage( new TestFinished( test, testMetrics.ExecutionTime, logger.ToString() ) );

                            executionMessageSink.OnMessage(
                                new TestCaseFinished(
                                    testCase,
                                    testMetrics.ExecutionTime,
                                    testMetrics.TestsRun,
                                    testMetrics.TestFailed,
                                    testMetrics.TestSkipped ) );

                            executionMessageSink.OnMessage(
                                new TestMethodFinished(
                                    new[] { testCase },
                                    testCase.TestMethod,
                                    testMetrics.ExecutionTime,
                                    testMetrics.TestsRun,
                                    testMetrics.TestFailed,
                                    testMetrics.TestSkipped ) );
                        };

                        testMetrics.OnTestsDiscovered( 1 );

                        if ( executionOptions.DisableParallelizationOrDefault() )
                        {
                            this._taskRunner.RunSynchronously(
                                () => this.RunTestAsync(
                                    executionMessageSink,
                                    projectReferences,
                                    directoryOptionsReader,
                                    testCase,
                                    test,
                                    testMetrics,
                                    logger ) );
                        }
                        else
                        {
                            var task = Task.Run(
                                () => this.RunTestAsync(
                                    executionMessageSink,
                                    projectReferences,
                                    directoryOptionsReader,
                                    testCase,
                                    test,
                                    testMetrics,
                                    logger ) );

                            // Throttle execution thanks to the semaphore.
                            semaphore.Wait();

                            // When the task is over, release the semaphore.
                            _ = task.ContinueWith( _ => semaphore.Release(), TaskScheduler.Current );

                            tasks.TryAdd( task, task );
                        }
                    }
                }
            }

            // Wait for all tasks to complete and catch exceptions.
#pragma warning disable VSTHRD002
            Task.WhenAll( tasks.Keys ).Wait();
#pragma warning restore VSTHRD002
        }

        private async Task RunTestAsync(
            IMessageSink executionMessageSink,
            TestProjectReferences projectReferences,
            TestDirectoryOptionsReader directoryOptionsReader,
            ITestCase testCase,
            Test test,
            Metrics testMetrics,
            TestOutputHelper logger )
        {
            var testStopwatch = Stopwatch.StartNew();

            try
            {
                testMetrics.OnTestStarted();

                var testInput = this._factory.TestInputFactory.FromFile( this._factory.ProjectProperties, directoryOptionsReader, testCase.UniqueID );

                var testOptions = new TestContextOptions
                {
                    References = projectReferences.MetadataReferences, RequireOrderedAspects = testInput.Options.RequireOrderedAspects.GetValueOrDefault()
                };

                if ( testInput.IsSkipped )
                {
                    executionMessageSink.OnMessage( new TestSkipped( test, testInput.SkipReason ) );

                    // This raises the messages on parent nodes and need to be called last.
                    testMetrics.OnTestSkipped();
                }
                else
                {
                    var testRunner = TestRunnerFactory.CreateTestRunner(
                        testInput,
                        this._serviceProvider.Underlying.WithUntypedService( typeof(ILoggerFactory), new XunitLoggerFactory( logger, false ) ),
                        projectReferences,
                        logger );

                    await testRunner.RunAndAssertAsync( testInput, testOptions );

                    executionMessageSink.OnMessage( new TestPassed( test, testMetrics.ExecutionTime, logger.ToString() ) );

                    // This raises the messages on parent nodes and need to be called last.
                    testMetrics.OnTestSucceeded( testStopwatch.Elapsed );
                }
            }
            catch ( Exception e )
            {
                IFailureInformation failureInformation;

                if ( e is AggregateException { InnerExceptions.Count: 1 } aggregateException )
                {
                    failureInformation = ExceptionUtility.ConvertExceptionToFailureInformation( aggregateException.InnerException );
                }
                else
                {
                    failureInformation = ExceptionUtility.ConvertExceptionToFailureInformation( e );
                }

                executionMessageSink.OnMessage(
                    new TestFailed(
                        test,
                        testMetrics.ExecutionTime,
                        logger.ToString(),
                        failureInformation.ExceptionTypes,
                        failureInformation.Messages,
                        failureInformation.StackTraces,
                        failureInformation.ExceptionParentIndices ) );

                // This will raise the events on parents, so it should be last.
                testMetrics.OnTestFailed( testStopwatch.Elapsed );
            }
        }
    }
}