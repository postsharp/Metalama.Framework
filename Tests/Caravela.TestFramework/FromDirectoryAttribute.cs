using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    public class FromDirectoryAttribute : DataAttribute
    {
        private static readonly HashSet<string> _excludedDirectoryNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "bin", "obj" };
        private readonly string _subdirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromDirectoryAttribute"/> class.
        /// </summary>
        /// <param name="subdirectory">The directory containing the tests to execute, relatively to the project directory.</param>
        public FromDirectoryAttribute( string subdirectory )
        {
            this._subdirectory = subdirectory;
        }

        public override IEnumerable<object[]> GetData( MethodInfo testMethod )
        {
            // To debug this method, comment out the next line:
            // Debugger.Launch();

            var projectDirectory = TestEnvironment.GetProjectDirectory( testMethod.DeclaringType!.Assembly );
            
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
                    if ( Path.GetFileName( testPath ).StartsWith( "_" ) )
                    {
                        continue;
                    }

                    tests.Add( new[] { Path.GetRelativePath( projectDirectory, testPath ) } );
                }
            }

            var absoluteDirectoryPath = Path.Combine( projectDirectory, this._subdirectory );
            AddTestsInDirectory( absoluteDirectoryPath );

            return tests;
        }
    }
}
