// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework.XunitFramework;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Metalama.TestFramework
{
    /// <summary>
    /// Reads the set of <see cref="TestAssemblyReference"/> from the project.
    /// </summary>
    internal static class TestAssemblyMetadataReader
    {
        private static readonly ConcurrentDictionary<string, TestAssemblyMetadata> _projectOptionsCache = new();

        public static TestAssemblyMetadata GetMetadata( IAssemblyInfo assembly )
            => _projectOptionsCache.GetOrAdd( assembly.AssemblyPath, _ => GetProjectOptionsCore( assembly ) );

        private static TestAssemblyMetadata GetProjectOptionsCore( IAssemblyInfo assembly )
        {
            ImmutableArray<TestAssemblyReference> GetAssemblyReferences()
            {
                var referencesAttribute = assembly
                    .GetCustomAttributes( typeof(AssemblyMetadataAttribute) )
                    .SingleOrDefault(
                        a => string.Equals( (string) a.GetConstructorArguments().First<object>(), "ReferenceAssemblyList", StringComparison.Ordinal ) );

                if ( referencesAttribute == null )
                {
                    throw new InvalidOperationException(
                        "The test assembly must have a single AssemblyMetadataAttribute with Key = \"ReferenceAssemblyList\"." );
                }

                TestDiscoverer testDiscoverer = new( assembly );
                var projectDirectory = testDiscoverer.GetTestProjectProperties().ProjectDirectory;

                var path = (string) referencesAttribute.GetConstructorArguments().ElementAt( 1 )!;

                var lines = File.ReadAllLines( path );

                return lines.Select( t => new TestAssemblyReference { Path = FilterReference( projectDirectory, t ) } ).ToImmutableArray();
            }

            bool GetMustLaunchDebugger()
            {
                var launchDebuggerAttribute = assembly
                    .GetCustomAttributes( typeof(AssemblyMetadataAttribute) )
                    .SingleOrDefault(
                        a => string.Equals( (string) a.GetConstructorArguments().First<object>(), "MetalamaDebugTestFramework", StringComparison.Ordinal ) );

                if ( launchDebuggerAttribute == null )
                {
                    return false;
                }

                var value = launchDebuggerAttribute.GetConstructorArguments().ElementAt( 1 );

                return value is string s && s.ToLowerInvariant().Trim() == "true";
            }

            string? GetGlobalUsingsFile()
            {
                var implicitUsingsAttribute = assembly
                    .GetCustomAttributes( typeof(AssemblyMetadataAttribute) )
                    .SingleOrDefault(
                        a => string.Equals( (string) a.GetConstructorArguments().First<object>(), "GlobalUsingsFile", StringComparison.Ordinal ) );

                if ( implicitUsingsAttribute == null )
                {
                    return null;
                }

                var value = (string?) implicitUsingsAttribute.GetConstructorArguments().ElementAt( 1 );

                return value;
            }

            return new TestAssemblyMetadata( GetMustLaunchDebugger(), GetAssemblyReferences(), GetGlobalUsingsFile() );
        }

        private static string FilterReference( string projectDirectory, string path )
        {
            path = Path.Combine( projectDirectory, path );

            var directory = Path.GetDirectoryName( path )!;
            var leafDirectory = Path.GetFileName( directory );

            if ( leafDirectory.Equals( "ref", StringComparison.OrdinalIgnoreCase ) )
            {
                // If we get a reference assembly, take the implementation assembly instead.
                var implementationPath = Path.Combine( Path.GetDirectoryName( directory )!, Path.GetFileName( path ) );

                if ( File.Exists( implementationPath ) )
                {
                    return implementationPath;
                }
            }

            return path;
        }
    }
}