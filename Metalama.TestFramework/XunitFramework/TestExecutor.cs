// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Testing;
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

namespace Metalama.TestFramework.XunitFramework
{
    internal class TestExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
    {
        private readonly TestFactory _factory;

        public TestExecutor( AssemblyName assemblyName )
        {
            var assembly = Assembly.Load( assemblyName );
            var assemblyInfo = new ReflectionAssemblyInfo( assembly );
            TestDiscoverer discoverer = new( assemblyInfo );
            var projectProperties = discoverer.GetTestProjectProperties();
            this._factory = new TestFactory( projectProperties, new TestDirectoryOptionsReader( projectProperties.ProjectDirectory ), assemblyInfo );
        }

        void IDisposable.Dispose() { }

        ITestCase ITestFrameworkExecutor.Deserialize( string value )
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

        void ITestFrameworkExecutor.RunTests(
            IEnumerable<ITestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions )
        {
            var hasLaunchedDebugger = false;
            var directoryOptionsReader = new TestDirectoryOptionsReader( this._factory.ProjectProperties.ProjectDirectory );

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
                        new TestCollectionFinished(
                            collection,
                            collection.Key,
                            collectionMetrics.ExecutionTime,
                            collectionMetrics.TestsRun,
                            collectionMetrics.TestFailed,
                            collectionMetrics.TestSkipped ) );

                    executionMessageSink.OnMessage(
                        new TestAssemblyFinished(
                            collection,
                            collection.Key.TestAssembly,
                            collectionMetrics.ExecutionTime,
                            collectionMetrics.TestsRun,
                            collectionMetrics.TestFailed,
                            collectionMetrics.TestSkipped ) );
                };

                var projectMetadata = TestAssemblyMetadataReader.GetMetadata( collection.Key.TestAssembly.Assembly );

                if ( projectMetadata.MustLaunchDebugger && !hasLaunchedDebugger )
                {
                    hasLaunchedDebugger = true;
                    Debugger.Launch();
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
                        var logger = new TestOutputHelper();

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

                        var task = Task.Run(
                            () => this.RunTestAsync( executionMessageSink, projectReferences, directoryOptionsReader, testCase, test, testMetrics, logger ) );

                        if ( executionOptions.DisableParallelizationOrDefault() )
                        {
                            task.Wait();
                        }
                        else
                        {
                            // Throttle execution thanks to the semaphore.
                            semaphore.Wait();

                            // When the task is over, release the semaphore.
                            task.ContinueWith( _ => semaphore.Release() );

                            tasks.TryAdd( task, task );
                        }
                    }
                }
            }

            // Wait for all tasks to complete and catch exceptions.
            Task.WhenAll( tasks.Keys ).Wait();
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

                using var testOptions = new TestProjectOptions( plugIns: projectReferences.PlugIns );
                using var testContext = new TestContext( testOptions );

                var testInput = TestInput.FromFile( this._factory.ProjectProperties, directoryOptionsReader, testCase.UniqueID );

                if ( testInput.IsSkipped )
                {
                    executionMessageSink.OnMessage( new TestSkipped( test, testInput.SkipReason ) );

                    testMetrics.OnTestSkipped();
                }
                else
                {
                    var testRunner = TestRunnerFactory.CreateTestRunner(
                        testInput,
                        testContext.ServiceProvider,
                        projectReferences,
                        logger );

                    await testRunner.RunAndAssertAsync( testInput );

                    testMetrics.OnTestSucceeded( testStopwatch.Elapsed );

                    executionMessageSink.OnMessage( new TestPassed( test, testMetrics.ExecutionTime, logger.ToString() ) );
                }
            }
            catch ( Exception e )
            {
                testMetrics.OnTestFailed( testStopwatch.Elapsed );

                IFailureInformation failureInformation;

                if ( e is AggregateException { InnerExceptions: { Count: 1 } } aggregateException )
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
            }
        }
    }
}