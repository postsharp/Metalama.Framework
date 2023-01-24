// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.AspectTesting.Licensing;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// Reads the set of <see cref="TestAssemblyReference"/> from the project.
    /// </summary>
    internal class TestAssemblyMetadataReader : ITestAssemblyMetadataReader
    {
        private static readonly ConcurrentDictionary<string, TestAssemblyMetadata> _projectOptionsCache = new();

        public TestAssemblyMetadata GetMetadata( IAssemblyInfo assembly )
        {
            return _projectOptionsCache.GetOrAdd( assembly.AssemblyPath, _ => GetMetadataCore( assembly ) );
        }

        private static TestAssemblyMetadata GetMetadataCore( IAssemblyInfo assembly )
        {
            string GetShortAssemblyName()
            {
#pragma warning disable CA1307 // Use string comparison parameter. 
                var commaPosition = assembly.Name.IndexOf( ',' );
#pragma warning restore CA1307

                return commaPosition > 0 ? assembly.Name.Substring( 0, commaPosition ) : assembly.Name;
            }

            IAttributeInfo? GetOptionalAssemblyMetadataAttribute( string key )
                => assembly
                    .GetCustomAttributes( typeof(AssemblyMetadataAttribute) )
                    .SingleOrDefault( a => string.Equals( (string) a.GetConstructorArguments().First<object>(), key, StringComparison.Ordinal ) );

            IAttributeInfo GetRequiredAssemblyMetadataAttribute( string key )
                => GetOptionalAssemblyMetadataAttribute( key )
                   ?? throw new InvalidOperationException( $"The test assembly must have an AssemblyMetadataAttribute with Key = \"{key}\"." );

            string? GetOptionalAssemblyMetadataValue( string key )
                => (string?) GetOptionalAssemblyMetadataAttribute( key )?.GetConstructorArguments()?.ElementAt( 1 );

            string GetRequiredAssemblyMetadataValue( string key )
                => (string) (GetRequiredAssemblyMetadataAttribute( key ).GetConstructorArguments()?.ElementAt( 1 )
                             ?? throw new InvalidOperationException( "The AssemblyMetadataAttribute with Key = \"{key}\" contains no value." ));

            bool GetBoolAssemblyMetadataValue( string key )
            {
                var value = GetOptionalAssemblyMetadataValue( key );

                return !string.IsNullOrEmpty( value ) && value.ToLowerInvariant().Trim() == "true";
            }

            string GetProjectDirectory() => GetRequiredAssemblyMetadataValue( "ProjectDirectory" );

            ImmutableArray<string> GetParserSymbols()
                => (GetOptionalAssemblyMetadataValue( "DefineConstants" ) ?? "")
                    .Split( ';' )
                    .SelectAsEnumerable( s => s.Trim() )
                    .Where( s => !string.IsNullOrEmpty( s ) )
                    .ToImmutableArray();

            string GetTargetFramework() => GetRequiredAssemblyMetadataValue( "TargetFramework" );

            ImmutableArray<TestAssemblyReference> GetAssemblyReferences( string projectDirectory, string propertyName )
            {
                var path = GetRequiredAssemblyMetadataValue( propertyName );
                var lines = File.ReadAllLines( path );

                return lines.SelectAsImmutableArray( t => new TestAssemblyReference { Path = FilterReference( projectDirectory, t ) } );
            }

            bool GetMustLaunchDebugger() => GetBoolAssemblyMetadataValue( "MetalamaDebugTestFramework" );

            string? GetGlobalUsingsFile() => GetOptionalAssemblyMetadataValue( "GlobalUsingsFile" );

            string? GetProjectLicense() => GetOptionalAssemblyMetadataValue( "MetalamaLicense" );

            bool GetIgnoreUserProfileLicenses() => GetBoolAssemblyMetadataValue( "MetalamaTestFrameworkIgnoreUserProfileLicenses" );

            TestFrameworkLicenseStatus GetLicense() => new( GetShortAssemblyName(), GetProjectLicense(), GetIgnoreUserProfileLicenses() );

            var projectDirectory = GetProjectDirectory();

            return new TestAssemblyMetadata(
                projectDirectory,
                GetParserSymbols(),
                GetTargetFramework(),
                GetMustLaunchDebugger(),
                GetAssemblyReferences( projectDirectory, "ReferenceAssemblyList" ),
                GetAssemblyReferences( projectDirectory, "AnalyzerAssemblyList" ),
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