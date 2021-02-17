using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.UnitTestFramework;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.Framework.Aspects.UnitTests
{
    public class FromDirectoryAttribute : DataAttribute
    {
       
        private static readonly HashSet<string> _excludedDirectoryNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "bin", "obj" };
        private readonly string _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromDirectoryAttribute"/> class.
        /// </summary>
        /// <param name="directory">The directory containing the tests to execute, relatively to the project directory.</param>
        public FromDirectoryAttribute( string directory )
        {
            var projectDir = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Single( a => a.Key == "ProjectDirectory" ).Value!;

            this._directory = Path.Combine( directory, projectDir );
        }

        
        public override IEnumerable<object[]> GetData( MethodInfo testMethod )
        {
            List<object[]> tests = new();

            void AddTestsInDirectory( string dirPath )
            {
                if ( _excludedDirectoryNames.Contains( Path.GetFileName( dirPath ) ) )
                {
                    return;
                }

                foreach ( var nestedDir in Directory.EnumerateDirectories( dirPath ) )
                {
                    AddTestsInDirectory( nestedDir );
                }

                foreach ( var testPath in Directory.EnumerateFiles( dirPath, "*.cs" ) )
                {
                    tests.Add( new[] { Path.GetRelativePath( this._directory, testPath ), testPath } );
                }
            }

            foreach ( var dir in Directory.EnumerateDirectories( this._directory ) )
            {
                AddTestsInDirectory( dir );
            }

            return tests;
        }

      
    }
}
