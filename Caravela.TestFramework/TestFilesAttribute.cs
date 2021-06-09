// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.XunitFramework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Provides test data to test theories by listing the test files in the current directory.
    /// </summary>
    public class TestFilesAttribute : DataAttribute
    {
        private readonly string _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFilesAttribute"/> class.
        /// </summary>
        /// <param name="subdirectory">Optionally, the directory containing the tests to execute, relatively to the caller source directory.
        /// If you use this parameter, you must pass the same value to the <see cref="CaravelaTestSuite.RunTest"/> method.
        /// </param>
        public TestFilesAttribute( string? subdirectory = null, [CallerFilePath] string? callerPath = null )
        {
            this._directory = Path.GetDirectoryName( callerPath )!;

            if ( subdirectory != null )
            {
                this._directory = Path.Combine( this._directory, subdirectory );
            }
        }

        public override IEnumerable<object[]> GetData( MethodInfo testMethod )
        {
            var discoverer = new TestDiscoverer( new ReflectionAssemblyInfo( testMethod.DeclaringType!.Assembly ) );
            
            foreach ( var testCase in discoverer.Discover( this._directory ) )
            {
                if ( testCase.SkipReason == null )
                {
                    var relativePath = Path.GetRelativePath( this._directory, testCase.FullPath );

                    yield return new object[] { relativePath };
                }
            }
        }
    }
}