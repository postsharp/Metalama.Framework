// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Framework.Tests.UnitTests.Utilities;
using Metalama.Testing.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public sealed class BuildTouchFileTests : UnitTestClass
{
    [Fact]
    public void TestExternalBuild()
    {
        const string aspectCodePart1 = @"
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public void IntroducedMethod_Void()
        {
";

        const string aspectCodeAddition = @"
            Console.WriteLine(""new line"");
";

        const string aspectCodePart2 = @"
            meta.Proceed();
        }
    }
";

        TestFileSystemWatcherFactory fileSystemWatcherFactory = new();
        var mocks = new AdditionalServiceCollection( fileSystemWatcherFactory );

        using var testContext = this.CreateTestContext( new TestContextOptions { HasBuildTouchFile = true }, mocks );

        Dictionary<string, DateTime> projectFilesTimestamps = new();

        var externalBuildStarted = false;

        TestFileSystemWatcher buildFileWatcher = new(
            Path.GetDirectoryName( testContext.ProjectOptions.BuildTouchFile ).AssertNotNull(),
            "*" + Path.GetExtension( testContext.ProjectOptions.BuildTouchFile ) );

        fileSystemWatcherFactory.Add( buildFileWatcher );

        var aspectCodePath = Path.Combine( testContext.ProjectOptions.ProjectDirectory, "Aspect.cs" );
        var class1CodePath = Path.Combine( testContext.ProjectOptions.ProjectDirectory, "Class1.cs" );

        var code = new Dictionary<string, string>
        {
            [aspectCodePath] = aspectCodePart1 + aspectCodePart2,
            [class1CodePath] =
                @"
    [Introduction]
    internal partial class TargetClass
    {
    }
"
        };

        foreach ( var fileName in code.Keys )
        {
            File.WriteAllText( fileName, "" );
            projectFilesTimestamps.Add( fileName, File.GetLastWriteTime( fileName ) );
        }

        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code );

        using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );
        var eventHub = pipelineFactory.ServiceProvider.GetRequiredService<AnalysisProcessEventHub>();

        using DesignTimeAspectPipeline pipeline = new(
            pipelineFactory,
            testContext.ProjectOptions,
            compilation1 );

        eventHub.ExternalBuildCompletedEvent.RegisterHandler(
            _ =>
            {
                Assert.False( externalBuildStarted );
                externalBuildStarted = true;
            } );

        // First compilation.
        Assert.True( pipeline.TryExecute( compilation1, default, out _ ) );
        Assert.Equal( DesignTimeAspectPipelineStatus.Ready, pipeline.Status );
        Assert.False( externalBuildStarted );

        // Project files should not be touched.
        foreach ( var fileName in code.Keys )
        {
            Assert.Equal( projectFilesTimestamps[fileName], File.GetLastWriteTime( fileName ) );
        }

        // Second compilation with changes.
        code[aspectCodePath] = aspectCodePart1 + aspectCodeAddition + aspectCodePart2;
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code );
        Assert.True( pipeline.TryExecute( compilation2, default, out _ ) );

        Assert.Equal( DesignTimeAspectPipelineStatus.Paused, pipeline.Status );
        Assert.True( pipelineFactory.EventHub.IsEditingCompileTimeCode );
        Assert.False( externalBuildStarted );

        // Simulate an external build.
        File.Create( testContext.ProjectOptions.BuildTouchFile! );

        buildFileWatcher.Notify(
            new FileSystemEventArgs( WatcherChangeTypes.Created, buildFileWatcher.Path, Path.GetFileName( testContext.ProjectOptions.BuildTouchFile ) ) );

        // The notification should have been received.
        Assert.True( externalBuildStarted );
        Assert.False( pipelineFactory.EventHub.IsEditingCompileTimeCode );
        Assert.Equal( DesignTimeAspectPipelineStatus.Default, pipeline.Status );

        // Aspect files should have been touched.
        foreach ( var fileName in code.Keys )
        {
            if ( fileName == aspectCodePath )
            {
                Assert.True( projectFilesTimestamps[fileName] < File.GetLastWriteTime( fileName ) );
            }
            else
            {
                Assert.False( projectFilesTimestamps[fileName] < File.GetLastWriteTime( fileName ) );
            }
        }

        Assert.True( pipeline.TryExecute( compilation2, default, out _ ) );
    }
}