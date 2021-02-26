// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    /// <summary>
    /// When applied on a test theory method specifies the directory from which to load the test source files for the given test theory method.
    /// </summary>
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

                    tests.Add( new object[] { Path.GetRelativePath( projectDirectory, testPath ) } );
                }
            }

            var absoluteDirectoryPath = Path.Combine( projectDirectory, this._subdirectory );
            AddTestsInDirectory( absoluteDirectoryPath );

            return tests;
        }
    }
}
