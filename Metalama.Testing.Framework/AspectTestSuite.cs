// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Testing.Api;
using Metalama.Testing.Api.Options;
using Metalama.Testing.Framework.Utilities;
using Metalama.Testing.Framework.XunitFramework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.Testing.Framework;

/// <summary>
/// A base class for test classes built using the current framework.
/// All test methods must be annotated with both <c>[Theory]</c> and <see cref="CurrentDirectoryAttribute"/>,
/// must have a single parameter accepting the relative path of the test file, and must call <see cref="RunTestAsync"/> as their only implementation.
/// </summary>
public abstract class AspectTestSuite
{
    static AspectTestSuite()
    {
        TestingServices.Initialize();
    }

    private record AssemblyAssets(
        TestProjectProperties ProjectProperties,
        TestDirectoryOptionsReader OptionsReader );

    private static readonly WeakCache<Assembly, AssemblyAssets> _cache = new();

    private static AssemblyAssets GetAssemblyAssets( Assembly assembly )
        => _cache.GetOrAdd(
            assembly,
            a =>
            {
                var assemblyInfo = new ReflectionAssemblyInfo( a );
                var discoverer = new TestDiscoverer( assemblyInfo );

                var projectProperties = discoverer.GetTestProjectProperties();

                return new AssemblyAssets(
                    projectProperties,
                    new TestDirectoryOptionsReader( projectProperties.ProjectDirectory ) );
            } );

    protected ITestOutputHelper Logger { get; }

    protected AspectTestSuite( ITestOutputHelper logger )
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
        var testSuiteAssembly = this.GetType().Assembly;
        var assemblyAssets = GetAssemblyAssets( testSuiteAssembly );
        var projectReferences = TestAssemblyMetadataReader.GetMetadata( new ReflectionAssemblyInfo( this.GetType().Assembly ) ).ToProjectReferences();

        var directory = this.GetDirectory( callerMemberName! );

        var fullPath = Path.Combine( directory, relativePath );

        this.Logger.WriteLine( "Test input file: " + fullPath );
        var projectRelativePath = PathUtil.GetRelativePath( assemblyAssets.ProjectProperties.ProjectDirectory, fullPath );

        var testInput = TestInput.FromFile( assemblyAssets.ProjectProperties, assemblyAssets.OptionsReader, projectRelativePath );

        using var testOptions = new TestProjectOptions(
            plugIns: projectReferences.PlugIns,
            requireOrderedAspects: testInput.Options.RequireOrderedAspects.GetValueOrDefault() );

        using var testContext = new TestContext( testOptions, projectReferences.MetadataReferences );

        var testRunner = TestRunnerFactory.CreateTestRunner( testInput, testContext.ServiceProvider, projectReferences, this.Logger );
        await testRunner.RunAndAssertAsync( testInput, testOptions );
    }
}