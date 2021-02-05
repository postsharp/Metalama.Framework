using Caravela.TestFramework;
using Caravela.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Aspects.UnitTests
{
    public class AspectUnitTest
    {
        public static readonly AspectTestsListTheoryData AspectTestsList = new AspectTestsListTheoryData( GetProjectDirectory() );
        private readonly ITestOutputHelper _logger;

        public AspectUnitTest( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        [Theory]
        [MemberData( nameof( AspectTestsList ) )]
        public async Task RunTestAsync( string relativeSourcePath )
        {
            var projectDir = GetProjectDirectory();
            var sourcePath = Path.Combine( projectDir, relativeSourcePath );
            var expectedTransformedPath = Path.Combine( Path.GetDirectoryName( sourcePath )!, Path.GetFileNameWithoutExtension( sourcePath ) + ".transformed.txt" );

            var testSource = await File.ReadAllTextAsync( sourcePath );
            var expectedTransformedSource = await File.ReadAllTextAsync( expectedTransformedPath );

            var testRunner = new AspectUnitTestRunner( this._logger );
            var testResult = await testRunner.Run( testSource );

            await SaveTransformedSourceAsync( projectDir, relativeSourcePath, testResult );

            testResult.AssertTransformedSource( expectedTransformedSource );
        }

        private static string GetProjectDirectory()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single( a => a.Key == "ProjectDirectory" ).Value!;
        }

        private static async Task SaveTransformedSourceAsync(string projectDir, string relativeSourcePath, TestResult testResult)
        {
            var outputDirPath = Path.Combine(
                projectDir,
                "obj\\transformed",
                Path.GetDirectoryName( relativeSourcePath )! );
            Directory.CreateDirectory( outputDirPath );

            var outputPath = Path.Combine( outputDirPath, Path.GetFileNameWithoutExtension( relativeSourcePath ) + ".transformed.txt" );
            await File.WriteAllTextAsync( outputPath, testResult.TemplateOutputSource?.ToString()?.Trim() );
        }
    }

    public class AspectTestsListTheoryData : TheoryData<string>
    {
        private static readonly HashSet<string> _excludedDirectoryNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "bin", "obj" };
        private readonly string _projectDir;

        public AspectTestsListTheoryData( string projectDir )
        {
            this._projectDir = projectDir;
            
            foreach ( var dir in Directory.EnumerateDirectories( projectDir ) )
            {
                this.AddTestsInDirectory( dir );
            }
        }

        private void AddTestsInDirectory( string dirPath )
        {
            if ( _excludedDirectoryNames.Contains( Path.GetFileName( dirPath ) ) )
            {
                return;
            }

            foreach ( var nestedDir in Directory.EnumerateDirectories( dirPath ) )
            {
                this.AddTestsInDirectory( nestedDir );
            }

            foreach ( var testPath in Directory.EnumerateFiles( dirPath, "*.cs" ) )
            {
                this.Add( Path.GetRelativePath( this._projectDir, testPath ) );
            }
        }
    }
}
