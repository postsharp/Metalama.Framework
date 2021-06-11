// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.XunitFramework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Provides test data to test theories by listing the test files in the current directory.
    /// </summary>
    public sealed class CurrentDirectoryAttribute : DataAttribute
    {
        /// <summary>
        /// Gets the root directory of test files for the current test methods.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentDirectoryAttribute"/> class.
        /// </summary>
        /// <param name="subdirectory">Optionally, the directory containing the tests to execute, relatively to the caller source directory.
        /// If you use this parameter, you must pass the same value to the <see cref="TestSuite.RunTest"/> method.
        /// </param>
        public CurrentDirectoryAttribute( string? subdirectory = null, [CallerFilePath] string? callerPath = null )
        {
            this.Directory = Path.GetDirectoryName( callerPath )!;

            if ( subdirectory != null )
            {
                this.Directory = Path.Combine( this.Directory, subdirectory );
            }
        }

        public override IEnumerable<object[]> GetData( MethodInfo testMethod )
        {
            var discoverer = new TestDiscoverer( new ReflectionAssemblyInfo( testMethod.DeclaringType!.Assembly ) );

            var excludedDirectories = ImmutableHashSet<string>.Empty;

            // Look at other methods to see if they cover specific subdirectories. It means that that they should be excluded from the current set.
            foreach ( var method in testMethod.DeclaringType.GetMethods( BindingFlags.Public | BindingFlags.Instance ) )
            {
                if ( method == testMethod )
                {
                    continue;
                }

                var testFilesAttribute = method.GetCustomAttribute<CurrentDirectoryAttribute>();

                if ( testFilesAttribute != null )
                {
                    excludedDirectories = excludedDirectories.Add( testFilesAttribute.Directory );
                }
            }

            foreach ( var testCase in discoverer.Discover( this.Directory, excludedDirectories ) )
            {
                if ( testCase.SkipReason == null )
                {
                    var relativePath = Path.GetRelativePath( this.Directory, testCase.FullPath );

                    yield return new object[] { relativePath };
                }
            }
        }
    }
}