// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.TestFramework.XunitFramework
{
    [Serializable]
    internal class TestDiscoverer : LongLivedMarshalByRefObject, ITestFrameworkDiscoverer
    {
        private static readonly HashSet<string> _excludedDirectoryNames = new( StringComparer.OrdinalIgnoreCase ) { "bin", "obj" };
        private readonly IAssemblyInfo _assembly;
        private readonly IMessageSink? _messageSink;

        public TestDiscoverer( IAssemblyInfo assembly, IMessageSink? messageSink = null )
        {
            this._assembly = assembly;
            this._messageSink = messageSink;

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

        public TestProjectProperties GetTestProjectProperties()
        {
            var customAttributes = this._assembly
                .GetCustomAttributes( typeof(AssemblyMetadataAttribute) )
                .ToList();

            string? GetValue( string key, bool required, string? defaultValue = null )
            {
                var attributes = customAttributes
                    .Where( a => string.Equals( (string) a.GetConstructorArguments().First(), key, StringComparison.Ordinal ) )
                    .ToList();

                if ( attributes.Count == 0 && !required )
                {
                    return defaultValue;
                }

                if ( attributes.Count != 1 )
                {
                    throw new InvalidOperationException(
                        $"The assembly '{this._assembly.AssemblyPath}' must have a single AssemblyMetadataAttribute with Key = \"{key}\" but it has {attributes.Count}." );
                }

                var value = (string?) attributes[0].GetConstructorArguments().ElementAt( 1 );

                if ( string.IsNullOrEmpty( value ) )
                {
                    if ( required )
                    {
                        throw new InvalidOperationException(
                            $"The assembly '{this._assembly.AssemblyPath}' AssemblyMetadataAttribute with Key = \"{key}\" has a null or empty value." );
                    }
                    else
                    {
                        return defaultValue;
                    }
                }

                return value;
            }

            var projectDirectory = GetValue( "ProjectDirectory", true ).NotNull();

            var parserSymbols = GetValue( "DefineConstants", false, "" )
                .NotNull()
                .Split( ';' )
                .Select( s => s.Trim() )
                .Where( s => !string.IsNullOrEmpty( s ) )
                .ToImmutableArray();

            var targetFramework = GetValue( "TargetFramework", true ).NotNull();

            return new TestProjectProperties( projectDirectory, parserSymbols, targetFramework );
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
            var sync = new object();

            this._messageSink?.Trace( $"Discovering tests in directory '{subDirectory}'." );

            var projectProperties = this.GetTestProjectProperties();
            TestDirectoryOptionsReader reader = new( projectProperties.ProjectDirectory );
            TestFactory factory = new( projectProperties, reader, this._assembly );

            ConcurrentBag<Task> tasks = new();
            var pendingTasks = new StrongBox<int>( 0 );

            void AddTestsInDirectory( string directory )
            {
                try
                {
                    // Skip bin, obj.
                    if ( _excludedDirectoryNames.Contains( Path.GetFileName( directory ) ) )
                    {
                        return;
                    }

                    var options = reader.GetDirectoryOptions( directory );

                    // If the directory is excluded, don't continue.
                    if ( options.Exclude.GetValueOrDefault() )
                    {
                        this._messageSink?.Trace( $"Child directory '{directory}' excluded because of the Exclude option." );

                        return;
                    }

                    this._messageSink?.Trace( $"Processing directory '{directory}'." );

                    // If the directory is included, index the files.
                    var runnerFileName = "_Runner.cs";

                    foreach ( var testPath in Directory.EnumerateFiles( directory, "*.cs" ) )
                    {
                        var fileName = Path.GetFileName( testPath );

                        if ( fileName[0] == '_' )
                        {
                            continue;
                        }

                        var firstDotPosition = fileName.IndexOfOrdinal( '.' );
                        var extension = fileName.Substring( firstDotPosition );

                        if ( !string.Equals( extension, ".cs", StringComparison.Ordinal ) )
                        {
                            // Skipping.

                            continue;
                        }

                        if ( Path.GetFileName( testPath ).Equals( runnerFileName, StringComparison.OrdinalIgnoreCase ) )
                        {
                            continue;
                        }

                        this._messageSink?.Trace( $"Including the file '{testPath}'" );

                        var testCase = new TestCase( factory, PathUtil.GetRelativePath( projectProperties.ProjectDirectory, testPath ) );

                        this._messageSink?.Trace(
                            $"    {((ITestCase) testCase).TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name} " +
                            $"/ {((ITestCase) testCase).TestMethod.TestClass.TestCollection.DisplayName}" +
                            $"/ {((ITestCase) testCase).TestMethod.TestClass.Class.Name} " +
                            $"/ {((ITestCase) testCase).TestMethod.Method.Name} " +
                            $"/ {((ITestCase) testCase).DisplayName}" );

                        // Somehow xunit does not like if we call onTestCaseDiscovered concurrently.
                        lock ( sync )
                        {
                            onTestCaseDiscovered( testCase );
                        }
                    }

                    // Process children directories.

                    foreach ( var nestedDir in Directory.EnumerateDirectories( directory ) )
                    {
                        if ( excludedSubdirectories.Contains( nestedDir ) )
                        {
                            this._messageSink?.Trace( $"Child directory '{nestedDir}' excluded because it is covered by other tests." );

                            continue;
                        }

                        if ( !isXUnitFrameworkDiscovery )
                        {
                            // Don't include a directory that has a _Runner file.
                            var runnerFile = Path.Combine( nestedDir, runnerFileName );

                            if ( File.Exists( runnerFile ) )
                            {
                                this._messageSink?.Trace( $"Child directory '{nestedDir}' excluded it contains '{runnerFileName}'." );

                                continue;
                            }
                        }

                        Interlocked.Increment( ref pendingTasks.Value );
                        tasks.Add( Task.Run( () => AddTestsInDirectory( nestedDir ) ) );
                    }
                }
                finally
                {
                    Interlocked.Decrement( ref pendingTasks.Value );
                }
            }

            Interlocked.Increment( ref pendingTasks.Value );
            AddTestsInDirectory( subDirectory ?? reader.ProjectDirectory );

            while ( pendingTasks.Value > 0 )
            {
                Task.WhenAll( tasks ).Wait();
            }
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

        string ITestFrameworkDiscoverer.TestFrameworkDisplayName => "Metalama";
    }
}