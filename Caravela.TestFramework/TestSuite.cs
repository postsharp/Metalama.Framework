// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    /// <summary>
    /// A base class for test classes built using the current framework.
    /// All test methods must be annotated with both <c>[Theory]</c> and <see cref="CurrentDirectoryAttribute"/>,
    /// must have a single parameter accepting the relative path of the test file, and must call <see cref="RunTestAsync"/> as their only implementation.
    /// </summary>
    public abstract class TestSuite
    {
        protected ITestOutputHelper Logger { get; }

        protected TestSuite( ITestOutputHelper logger )
        {
            this.Logger = logger;
        }

        protected virtual string GetDirectory( string callerMemberName )
        {
            var callerMethod = this
                .GetType()
                .GetMethods( BindingFlags.Instance | BindingFlags.Public )
                .Single( m => m.Name == callerMemberName );

            var testFilesAttribute = callerMethod.GetCustomAttribute<CurrentDirectoryAttribute>();

            if ( testFilesAttribute == null )
            {
                throw new InvalidOperationException( "The calling method does not have a [TestFiles] attribute." );
            }

            return testFilesAttribute.Directory;
        }

        /// <summary>
        /// Executes a test.
        /// </summary>
        /// <param name="relativePath">Relative path of the file relatively to the directory of the caller code.</param>
        protected async Task RunTestAsync( string relativePath, [CallerMemberName] string? callerMemberName = null )
        {
            var directory = this.GetDirectory( callerMemberName! );
            using var testOptions = new TestProjectOptions();
            var serviceProvider = ServiceProviderFactory.GetServiceProvider( testOptions );

            var directoryOptionsReader = TestDirectoryOptionsReader.GetInstance( this.GetType().Assembly );

            var fullPath = Path.Combine( directory, relativePath );

            this.Logger.WriteLine( "Test input file: " + fullPath );
            var projectRelativePath = Path.GetRelativePath( directoryOptionsReader.ProjectDirectory, fullPath );

            var testInput = TestInput.FromFile( directoryOptionsReader, projectRelativePath );
            testInput.Options.References.AddRange( TestAssemblyReferenceReader.GetAssemblyReferences( new ReflectionAssemblyInfo( this.GetType().Assembly ) ) );
            var testRunner = TestRunnerFactory.CreateTestRunner( testInput, serviceProvider, this.Logger );
            await testRunner.RunAndAssertAsync( testInput );
        }
    }
}