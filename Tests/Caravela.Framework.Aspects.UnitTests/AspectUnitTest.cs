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
            string projectDir = GetProjectDirectory();
            string sourcePath = Path.Combine( projectDir, relativeSourcePath );
            string expectedTransformedPath = Path.Combine( Path.GetDirectoryName( sourcePath ), Path.GetFileNameWithoutExtension( sourcePath ) + ".transformed.txt" );

            string testSource = await File.ReadAllTextAsync( sourcePath );
            string expectedTransformedSource = await File.ReadAllTextAsync( expectedTransformedPath );

            var testRunner = new AspectUnitTestRunner( this._logger );
            var testResult = await testRunner.Run( testSource );

            var targetTextSpan = TestSyntaxHelper.FindRegionSpan( testResult.TransformedTargetSyntax, "Target" );

            if ( targetTextSpan != null )
            {
                string sourceText = testResult.TransformedTargetSource.GetSubText( targetTextSpan.Value ).ToString()?.Trim();
                await SaveTransformedTargetAsync( Path.Combine( projectDir, "obj\\transformed" ), relativeSourcePath, sourceText );
                testResult.AssertTransformedSourceSpan( expectedTransformedSource, targetTextSpan.Value );
            }
            else
            {
                await SaveTransformedTargetAsync( Path.Combine( projectDir, "obj\\transformed" ), relativeSourcePath, testResult.TransformedTargetSource?.ToString()?.Trim() );
                testResult.AssertTransformedSource( expectedTransformedSource );
            }
        }

        private static string GetProjectDirectory()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single( a => a.Key == "ProjectDirectory" ).Value;
        }

        private static async Task SaveTransformedTargetAsync( string rootDir, string relativeSourcePath, string sourceText )
        {
            string outputDirPath = Path.Combine( rootDir, Path.GetDirectoryName( relativeSourcePath ) );
            Directory.CreateDirectory( outputDirPath );

            string outputPath = Path.Combine( outputDirPath, Path.GetFileNameWithoutExtension( relativeSourcePath ) + ".transformed.txt" );
            await File.WriteAllTextAsync( outputPath, sourceText );
        }
    }

    public class AspectTestsListTheoryData : TheoryData<string>
    {
        private static readonly HashSet<string> ExcludedDirectoryNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "bin", "obj" };
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
            if ( ExcludedDirectoryNames.Contains( Path.GetFileName( dirPath ) ) ) return;

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
