// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework.XunitFramework
{
    internal class TestDiscoverer : ITestFrameworkDiscoverer
    {
        private static readonly HashSet<string> _excludedDirectoryNames = new( StringComparer.OrdinalIgnoreCase ) { "bin", "obj" };
        private readonly IAssemblyInfo _assembly;

        public TestDiscoverer( IAssemblyInfo assembly )
        {
            this._assembly = assembly;

            var attrib = assembly.GetCustomAttributes( typeof(TargetFrameworkAttribute) ).FirstOrDefault();

            if ( attrib != null )
            {
                this.TargetFramework = attrib.GetConstructorArguments().Cast<string>().First();
            }
            else
            {
                this.TargetFramework = null!;
            }
        }

        void IDisposable.Dispose() { }

        public string FindProjectDirectory()
        {
            var projectDirectoryAttributes = this._assembly
                .GetCustomAttributes( typeof(AssemblyMetadataAttribute) )
                .Where( a => (string) a.GetConstructorArguments().First() == "ProjectDirectory" )
                .ToList();

            var projectDirectoryAttribute = projectDirectoryAttributes.FirstOrDefault();

            if ( projectDirectoryAttribute == null )
            {
                throw new InvalidOperationException(
                    $"The assembly '{this._assembly.AssemblyPath}' must have a single AssemblyMetadataAttribute with Key = \"ProjectDirectory\"." );
            }

            var value = (string?) projectDirectoryAttribute.GetConstructorArguments().ElementAt( 1 );

            if ( string.IsNullOrEmpty( value ) )
            {
                throw new InvalidOperationException(
                    "The project directory cannot be null or empty."
                    + " The project directory is stored as a value of the AssemblyMetadataAttribute with Key = \"ProjectDirectory\"." );
            }

            return value;
        }

        public List<TestCase> Discover( string subDirectory, ImmutableHashSet<string> excludedDirectories )
        {
            List<TestCase> testCases = new();
            this.Discover( c => testCases.Add( c ), subDirectory, false, excludedDirectories );

            return testCases;
        }

        private void Discover(
            Action<TestCase> onTestCaseDiscovered,
            string? subDirectory,
            bool isXUnitFrameworkDiscovery,
            ImmutableHashSet<string> excludedSubdirectories )
        {
            var baseDirectory = this.FindProjectDirectory();
            TestDirectoryOptionsReader reader = new( baseDirectory );
            TestFactory factory = new( reader, this._assembly );

            void AddTestsInDirectory( string directory )
            {
                var options = reader.GetDirectoryOptions( directory );

                // If the directory is excluded, don't continue.
                if ( options.Exclude.GetValueOrDefault() ||
                     _excludedDirectoryNames.Contains( Path.GetFileName( directory ) ) )
                {
                    return;
                }

                // Process children directories.
                var runnerFileName = "_Runner.cs";

                foreach ( var nestedDir in Directory.EnumerateDirectories( directory ) )
                {
                    if ( excludedSubdirectories.Contains( nestedDir ) )
                    {
                        continue;
                    }

                    if ( !isXUnitFrameworkDiscovery )
                    {
                        // Don't include a directory that has a _Runner file.
                        var runnerFile = Path.Combine( nestedDir, runnerFileName );

                        if ( File.Exists( runnerFile ) )
                        {
                            continue;
                        }
                    }

                    AddTestsInDirectory( nestedDir );
                }

                // If the directory is included, index the files.
                foreach ( var testPath in Directory.EnumerateFiles( directory, "*.cs" ) )
                {
                    if ( Path.GetFileName( testPath ).Equals( runnerFileName, StringComparison.OrdinalIgnoreCase )
                         || testPath.EndsWith( FileExtensions.TransformedCode, StringComparison.OrdinalIgnoreCase ) )
                    {
                        continue;
                    }

                    var testCase = new TestCase( factory, Path.GetRelativePath( baseDirectory, testPath ) );
                    onTestCaseDiscovered( testCase );
                }
            }

            AddTestsInDirectory( subDirectory ?? reader.ProjectDirectory );
        }

        void ITestFrameworkDiscoverer.Find( bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions )
        {
            this.Discover( testCase => discoveryMessageSink.OnMessage( new TestCaseDiscoveryMessage( testCase ) ), null, true, ImmutableHashSet<string>.Empty );
            discoveryMessageSink.OnMessage( new DiscoveryCompleteMessage() );
        }

        void ITestFrameworkDiscoverer.Find(
            string typeName,
            bool includeSourceInformation,
            IMessageSink discoveryMessageSink,
            ITestFrameworkDiscoveryOptions discoveryOptions )
            => throw new NotImplementedException();

        string ITestFrameworkDiscoverer.Serialize( ITestCase testCase ) => testCase.UniqueID;

        public string TargetFramework { get; }

        string ITestFrameworkDiscoverer.TestFrameworkDisplayName => "Caravela";
    }
}