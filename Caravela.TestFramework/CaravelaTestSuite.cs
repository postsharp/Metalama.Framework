// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// A base class for test suites built using the current framework.
    /// All test methods must be annotated with both <see cref="TheoryAttribute"/> and <see cref="TestFilesAttribute"/>,
    /// must have a single parameter accepting the relative path of the test file, and must call <see cref="RunTest"/> as their only implementation.
    /// </summary>
    public class CaravelaTestSuite
    {
        private readonly ITestOutputHelper _logger;

        public CaravelaTestSuite( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        /// <summary>
        /// Executes a test.
        /// </summary>
        /// <param name="relativePath">Relative path of the file relatively to the caller directory and <paramref name="subdirectory"/>.</param>
        /// <param name="subdirectory">An optional directory, relatively to the caller directory.</param>
        protected void RunTest( string relativePath, string? subdirectory = null, [CallerFilePath] string? callerFilePath = null )
        {
            if ( subdirectory != null )
            {
                relativePath = Path.Combine( subdirectory, relativePath );
            }
            
            using var testOptions = new TestProjectOptions();
            using var serviceProvider = ServiceProviderFactory.GetServiceProvider( testOptions );
            
            var directoryOptionsReader = TestDirectoryOptionsReader.GetInstance( this.GetType().Assembly );
            var fullPath = Path.Combine( Path.GetDirectoryName( callerFilePath )!, relativePath );
            
            this._logger.WriteLine( "Test file: " + fullPath );
            var projectRelativePath = Path.GetRelativePath( directoryOptionsReader.ProjectDirectory, fullPath );

            var testInput = TestInput.FromFile( directoryOptionsReader, projectRelativePath );
            var testRunner = TestRunnerFactory.CreateTestRunner( testInput, serviceProvider );
            var testResult = testRunner.RunTest( testInput );
            testRunner.ExecuteAssertions( testInput, testResult, this._logger );
        }
    }
}