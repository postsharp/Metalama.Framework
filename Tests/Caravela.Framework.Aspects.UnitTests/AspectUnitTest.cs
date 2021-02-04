using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Aspects.UnitTests
{
    public class AspectUnitTest
    {
        public static AspectTestsListTheoryData AspectTestsList = new AspectTestsListTheoryData();
        private readonly ITestOutputHelper _logger;

        public AspectUnitTest( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        [Theory]
        [MemberData( nameof( AspectTestsList ) )]
        public async Task RunTestAsync( string sourcePath )
        {
            string expectedTransformedPath = Path.Combine( Path.GetDirectoryName( sourcePath ), Path.GetFileNameWithoutExtension( sourcePath ) + ".transformed.txt" );

            string testSource = await File.ReadAllTextAsync( sourcePath );
            string expectedTransformedSource = await File.ReadAllTextAsync( expectedTransformedPath );

            var testRunner = new AspectUnitTestRunner( this._logger );
            var testResult = await testRunner.Run( testSource );

            testResult.AssertTransformedSource( expectedTransformedSource );
        }
    }

    public class AspectTestsListTheoryData : TheoryData<string>
    {
        private static readonly HashSet<string> ExcludedDirectoryNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "bin", "obj" };

        public AspectTestsListTheoryData()
        {
            string projectDir = @"C:\src\Caravela2\Tests\Caravela.Framework.Aspects.UnitTests";

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
                this.Add( testPath );
            }
        }
    }
}
