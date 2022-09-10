// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.TestFramework.Licensing;
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
            => _projectOptionsCache.GetOrAdd( assembly.AssemblyPath, _ => GetMetadataCore( assembly ) );

        private static TestAssemblyMetadata GetMetadataCore( IAssemblyInfo assembly )
        {
            string GetShortAssemblyName()
            {
                var commanPosition = assembly.Name.IndexOf(',');
                return commanPosition > 0 ? assembly.Name.Substring( 0, commanPosition ) : assembly.Name;
            }

            IAttributeInfo? GetAssemblyMetadataAttribute( string key )
                => assembly
                   .GetCustomAttributes( typeof( AssemblyMetadataAttribute ) )
                   .SingleOrDefault( a => string.Equals( (string) a.GetConstructorArguments().First<object>(), key, StringComparison.Ordinal ) );

            string? GetAssemblyMetadataValue( string key )
                => (string?) GetAssemblyMetadataAttribute( key )?.GetConstructorArguments()?.ElementAt( 1 );

            ImmutableArray<TestAssemblyReference> GetAssemblyReferences( string propertyName )
            {
                var path = GetAssemblyMetadataValue( propertyName );

                if ( path == null )
                {
                    throw new InvalidOperationException( $"The test assembly must have a AssemblyMetadataAttribute with Key = \"{propertyName}\"." );
                }

                TestDiscoverer testDiscoverer = new( assembly );
                var projectDirectory = testDiscoverer.GetTestProjectProperties().ProjectDirectory;

                var lines = File.ReadAllLines( path );

                return lines.Select( t => new TestAssemblyReference { Path = FilterReference( projectDirectory, t ) } ).ToImmutableArray();
            }

            bool GetMustLaunchDebugger()
            {
                var value = GetAssemblyMetadataValue( "MetalamaDebugTestFramework" );

                return !string.IsNullOrEmpty( value ) && value!.ToLowerInvariant().Trim() == "true";
            }

            string? GetGlobalUsingsFile()
                => GetAssemblyMetadataValue( "GlobalUsingsFile" );

            TestFrameworkLicenseStatus GetLicense()
                => new TestFrameworkLicenseStatus( GetShortAssemblyName(), GetAssemblyMetadataValue( "MetalamaLicense" ) );

            return new TestAssemblyMetadata(
                GetMustLaunchDebugger(),
                GetAssemblyReferences( "ReferenceAssemblyList" ),
                GetAssemblyReferences( "AnalyzerAssemblyList" ),
                GetGlobalUsingsFile(),
                GetLicense() );
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