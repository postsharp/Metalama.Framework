// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Tests.UnitTests.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class BuildTouchFileTests : TestBase
    {
        private class BuildTouchFileTestsProjectOptions : TestProjectOptions
        {
            public override string? BuildTouchFile { get; }

            public override bool IsTest => true;

            public BuildTouchFileTestsProjectOptions()
            {
                this.BuildTouchFile = Path.Combine( this.BaseDirectory, "touch.build" );
            }
        }

        [Fact]
        public void TestTouchFile()
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
            var mocks = new TestServiceCollection( fileSystemWatcherFactory );

            using var testContext = this.CreateTestContext( new BuildTouchFileTestsProjectOptions(), mocks );

            Dictionary<string, DateTime> projectFilesTimestamps = new();

            var externalBuildStarted = false;

            TestFileSystemWatcher buildFileWatcher = new( Path.GetDirectoryName( testContext.ProjectOptions.BuildTouchFile ).AssertNotNull(), "*.build" );
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

            var compilation1 = CreateCSharpCompilation( code );

            using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

            using DesignTimeAspectPipeline pipeline = new(
                pipelineFactory,
                testContext.ProjectOptions,
                compilation1 );

            pipeline.ExternalBuildCompletedEvent.RegisterHandler(
                _ =>
                {
                    Assert.False( externalBuildStarted );
                    externalBuildStarted = true;
                } );

            Assert.True( pipeline.TryExecute( compilation1, default, out _ ) );

            Assert.False( externalBuildStarted );

            foreach ( var fileName in code.Keys )
            {
                Assert.Equal( projectFilesTimestamps[fileName], File.GetLastWriteTime( fileName ) );
            }

            code[aspectCodePath] = aspectCodePart1 + aspectCodeAddition + aspectCodePart2;

            var compilation2 = CreateCSharpCompilation( code );
            Assert.True( pipeline.TryExecute( compilation2, default, out _ ) );

            Assert.False( externalBuildStarted );

            File.Create( testContext.ProjectOptions.BuildTouchFile! );

            buildFileWatcher.Notify(
                new FileSystemEventArgs( WatcherChangeTypes.Created, buildFileWatcher.Path, Path.GetFileName( testContext.ProjectOptions.BuildTouchFile ) ) );

            Assert.True( externalBuildStarted );

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
}