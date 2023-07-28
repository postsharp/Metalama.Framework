// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.AspectTesting;
using Metalama.Testing.AspectTesting.XunitFramework;
using Metalama.Testing.UnitTesting;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.TestFramework;

public sealed class TestExecutorTests : UnitTestClass
{
    [Theory]
    [InlineData(
        "Error!",
        "**ERROR**",
        "TestCollectionStarting,TestAssemblyStarting,TestClassStarting,TestMethodStarting,TestCaseStarting,TestStarting,TestFailed,TestFinished,TestCaseFinished,TestMethodFinished,TestClassFinished,TestAssemblyFinished,TestCollectionFinished" )]
    [InlineData(
        "/* Empty */",
        "// --- No output compilation units ---",
        "TestCollectionStarting,TestAssemblyStarting,TestClassStarting,TestMethodStarting,TestCaseStarting,TestStarting,TestPassed,TestFinished,TestCaseFinished,TestMethodFinished,TestClassFinished,TestAssemblyFinished,TestCollectionFinished" )]
    [InlineData(
        "// @Skipped",
        "",
        "TestCollectionStarting,TestAssemblyStarting,TestClassStarting,TestMethodStarting,TestCaseStarting,TestStarting,TestSkipped,TestFinished,TestCaseFinished,TestMethodFinished,TestClassFinished,TestAssemblyFinished,TestCollectionFinished" )]
    public void EventSequence( string testInput, string expectedTestOutput, string expectedEventSequence )
    {
        using var testContext = this.CreateTestContext();
        var fileSystem = new TestFileSystem( testContext.ServiceProvider.Underlying );
        const string directory = "C:\\test";
        fileSystem.CreateDirectory( directory );
        fileSystem.WriteAllText( Path.Combine( directory, "Test.cs" ), testInput );
        fileSystem.WriteAllText( Path.Combine( directory, "Test.t.cs" ), expectedTestOutput );

        var serviceProvider = (GlobalServiceProvider) testContext.ServiceProvider.Global.Underlying
            .WithUntypedService( typeof(IFileSystem), fileSystem )
            .WithService( new FakeMetadataReader( directory ) );

        var testProperties = new TestProjectProperties( directory, ImmutableArray<string>.Empty, "net6.0", ImmutableArray<string>.Empty );
        var assemblyInfo = new TestAssemblyInfo( $"test.dll" );
        var testFactory = new TestFactory( serviceProvider, testProperties, new TestDirectoryOptionsReader( serviceProvider, directory ), assemblyInfo );
        var messageSink = new TestMessageSink();

        var testExecutor = new TestExecutor( serviceProvider, testFactory );
        var testDiscoverer = new TestDiscoverer( serviceProvider, assemblyInfo );
        var tests = testDiscoverer.Discover( directory, ImmutableHashSet<string>.Empty );
        testExecutor.RunTests( tests, messageSink, new TestFrameworkExecutionOptions() );

        var sequence = string.Join( ",", messageSink.Messages.SelectAsEnumerable( x => x.GetType().Name ) );

        Assert.Equal( expectedEventSequence, sequence );
    }
}