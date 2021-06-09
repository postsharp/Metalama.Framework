// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework.XunitFramework
{
    internal class TestExecutor : ITestFrameworkExecutor
    {
        private readonly AssemblyName _assemblyName;
        private readonly TestFactory _factory;

        public TestExecutor( AssemblyName assemblyName )
        {
            this._assemblyName = assemblyName;
            var assembly = Assembly.Load( assemblyName );
            var assemblyInfo = new ReflectionAssemblyInfo( assembly );
            TestDiscoverer discoverer = new( assemblyInfo );
            this._factory = new TestFactory( new TestDirectoryOptionsReader( discoverer.FindProjectDirectory() ), assemblyInfo );
        }

        void IDisposable.Dispose() { }

        ITestCase ITestFrameworkExecutor.Deserialize( string value ) => new TestCase( this._factory, value );

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
            var directoryOptionsReader = new TestDirectoryOptionsReader( this._factory.BaseDirectory );

            var collections = testCases.GroupBy( t => t.TestMethod.TestClass.TestCollection );

            foreach ( var collection in collections )
            {
                executionMessageSink.OnMessage( new TestCollectionStarting( collection, collection.Key ) );

                executionMessageSink.OnMessage(
                    new TestAssemblyStarting(
                        collection,
                        collection.Key.TestAssembly,
                        DateTime.Now,
                        "CompileTime",
                        "CompileTime" ) );

                int collectionTestsRun = 0, collectionTestFailed = 0, collectionTestSkipped = 0;
                var collectionStopwatch = Stopwatch.StartNew();

                try
                {
                    foreach ( var type in collection.GroupBy( c => c.TestMethod.TestClass ) )
                    {
                        int typeTestsRun = 0, typeTestFailed = 0, typeTestSkipped = 0;
                        var typeStopwatch = Stopwatch.StartNew();

                        executionMessageSink.OnMessage( new TestClassStarting( type, type.Key ) );

                        try
                        {
                            foreach ( var testCase in type )
                            {
                                int testsRun = 0, testFailed = 0, testSkipped = 0;
                                var test = new Test( testCase );

                                executionMessageSink.OnMessage( new TestMethodStarting( new[] { testCase }, testCase.TestMethod ) );
                                executionMessageSink.OnMessage( new TestCaseStarting( testCase ) );
                                executionMessageSink.OnMessage( new TestStarting( test ) );

                                var testStopwatch = Stopwatch.StartNew();

                                var logger = new TestOutputHelper();

                                try
                                {
                                    using var testOptions = new TestProjectOptions();
                                    using var serviceProvider = ServiceProviderFactory.GetServiceProvider( testOptions );
                                    var testInput = TestInput.FromFile( directoryOptionsReader, testCase.UniqueID );
                                    if ( testInput.Options.SkipReason != null )
                                    {
                                        executionMessageSink.OnMessage( new TestSkipped( test, testInput.Options.SkipReason ) );

                                        testSkipped++;
                                        typeTestSkipped++;
                                        collectionTestSkipped++;
                                    }
                                    else
                                    {
                                        var testRunner = TestRunnerFactory.CreateTestRunner( testInput, serviceProvider );
                                        var testResult = testRunner.RunTest( testInput );
                                        testRunner.ExecuteAssertions( testInput, testResult, logger );

                                        executionMessageSink.OnMessage( new TestPassed( test, testStopwatch.GetSeconds(), logger.ToString() ) );

                                        testsRun++;
                                        typeTestsRun++;
                                        collectionTestsRun++;
                                    }
                                }
                                catch ( Exception e )
                                {
                                    testFailed++;
                                    typeTestFailed++;
                                    collectionTestFailed++;

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
                                            testStopwatch.GetSeconds(),
                                            logger.ToString(),
                                            failureInformation.ExceptionTypes,
                                            failureInformation.Messages,
                                            failureInformation.StackTraces,
                                            failureInformation.ExceptionParentIndices ) );
                                }
                                finally
                                {
                                    executionMessageSink.OnMessage( new TestFinished( test, testStopwatch.GetSeconds(), logger.ToString() ) );

                                    executionMessageSink.OnMessage(
                                        new TestCaseFinished( testCase, testStopwatch.GetSeconds(), testsRun, testFailed, testSkipped ) );

                                    executionMessageSink.OnMessage(
                                        new TestMethodFinished(
                                            new[] { testCase },
                                            testCase.TestMethod,
                                            testStopwatch.GetSeconds(),
                                            testsRun,
                                            testFailed,
                                            testSkipped ) );
                                }
                            }
                        }
                        finally
                        {
                            executionMessageSink.OnMessage(
                                new TestClassFinished(
                                    type,
                                    type.Key,
                                    typeStopwatch.GetSeconds(),
                                    typeTestsRun,
                                    typeTestFailed,
                                    typeTestSkipped ) );
                        }
                    }
                }
                finally
                {
                    executionMessageSink.OnMessage(
                        new TestCollectionFinished(
                            collection,
                            collection.Key,
                            collectionStopwatch.GetSeconds(),
                            collectionTestsRun,
                            collectionTestFailed,
                            collectionTestSkipped ) );

                    executionMessageSink.OnMessage(
                        new TestAssemblyFinished(
                            collection,
                            collection.Key.TestAssembly,
                            collectionStopwatch.GetSeconds(),
                            collectionTestsRun,
                            collectionTestFailed,
                            collectionTestSkipped ) );
                }
            }
        }
    }
}