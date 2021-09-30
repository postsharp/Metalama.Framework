// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Tests.UnitTests.Utilities;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
{
    public class BuildTouchFileTests : TestBase
    {
        private class BuildTouchFileTestsProjectOptions : TestProjectOptions
        {
            public override string? BuildTouchFile { get; }

            public BuildTouchFileTestsProjectOptions()
            {
                this.BuildTouchFile = Path.Combine( this.BaseTestDirectory, "touch.build" );
            }
        }

        public BuildTouchFileTests() : base( new BuildTouchFileTestsProjectOptions() ) { }

        [Fact]
        public void TestTouchFile()
        {
            const string aspectCodePart1 = @"
using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
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

            Dictionary<string, DateTime> projectFilesTimestamps = new();

            var externalBuildStarted = false;

            TestFileSystemWatcher buildFileWatcher = new( Path.GetDirectoryName( this.ProjectOptions.BuildTouchFile )!, "*.build" );
            TestFileSystemWatcherFactory fileSystemWatcherFactory = new();
            fileSystemWatcherFactory.Add( buildFileWatcher );

            var aspectCodePath = Path.Combine( this.ProjectOptions.ProjectDirectory, "Aspect.cs" );
            var class1CodePath = Path.Combine( this.ProjectOptions.ProjectDirectory, "Class1.cs" );

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
                File.Create( fileName );
                projectFilesTimestamps.Add( fileName, File.GetLastWriteTime( fileName ) );
            }

            var compilation = CreateCSharpCompilation( code );

            using var domain = new UnloadableCompileTimeDomain();

            using DesignTimeAspectPipeline pipeline = new(
                this.ProjectOptions,
                domain,
                true,
                directoryOptions: this.ProjectOptions,
                assemblyLocator: null,
                fileSystemWatcherFactory: fileSystemWatcherFactory );

            pipeline.ExternalBuildStarted += ( _, _ ) =>
            {
                Assert.False( externalBuildStarted );
                externalBuildStarted = true;
            };

            var syntaxTree = compilation.SyntaxTrees.Single( t => t.FilePath == class1CodePath );
            var result = pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree ), CancellationToken.None );

            Assert.True( result.Success );
            Assert.False( externalBuildStarted );

            foreach ( var fileName in code.Keys )
            {
                Assert.Equal( projectFilesTimestamps[fileName], File.GetLastWriteTime( fileName ) );
            }

            code[aspectCodePath] = aspectCodePart1 + aspectCodeAddition + aspectCodePart2;

            compilation = CreateCSharpCompilation( code );
            syntaxTree = compilation.SyntaxTrees.Single( t => t.FilePath == class1CodePath );
            result = pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree ), CancellationToken.None );

            Assert.False( result.Success );
            Assert.False( externalBuildStarted );

            File.Create( this.ProjectOptions.BuildTouchFile! );

            buildFileWatcher.Notify(
                new FileSystemEventArgs( WatcherChangeTypes.Created, buildFileWatcher.Path, Path.GetFileName( this.ProjectOptions.BuildTouchFile ) ) );

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

            result = pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree ), CancellationToken.None );
            Assert.True( result.Success );
        }
    }
}