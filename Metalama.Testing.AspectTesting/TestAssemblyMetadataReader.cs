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
    internal sealed class TestAssemblyMetadataReader : ITestAssemblyMetadataReader
    {
        private static readonly ConcurrentDictionary<string, TestAssemblyMetadata> _projectOptionsCache = new();

        public TestAssemblyMetadata GetMetadata( IAssemblyInfo assembly )
        {
            return _projectOptionsCache.GetOrAdd( assembly.AssemblyPath, static ( _, a ) => GetMetadataCore( a ), assembly );
        }

        private static TestAssemblyMetadata GetMetadataCore( IAssemblyInfo assembly )
        {
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

            string GetSourceDirectory() => GetOptionalAssemblyMetadataValue( "SourceDirectory" ) ?? GetProjectDirectory();

            string GetProjectPath() => GetRequiredAssemblyMetadataValue( "ProjectPath" );

            string GetProjectName() => Path.GetFileNameWithoutExtension( GetProjectPath() );

            ImmutableArray<string> GetParserSymbols()
                => (GetOptionalAssemblyMetadataValue( "DefineConstants" ) ?? "")
                    .Split( ';' )
                    .SelectAsReadOnlyList( s => s.Trim() )
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

            TestFrameworkLicenseStatus GetLicense() => new( GetProjectName(), GetProjectLicense(), GetIgnoreUserProfileLicenses() );

            ImmutableArray<string> GetIgnoredWarnings()
                => GetOptionalAssemblyMetadataValue( "IgnoredWarnings" )
                    ?.Split( ';' )
                    .Select( s => s.Trim() )
                    .Where( s => !string.IsNullOrEmpty( s ) )
                    .ToImmutableArray() ?? ImmutableArray<string>.Empty;

            var projectDirectory = GetProjectDirectory();

            return new TestAssemblyMetadata(
                projectDirectory,
                GetSourceDirectory(),
                GetParserSymbols(),
                GetTargetFramework(),
                GetMustLaunchDebugger(),
                GetAssemblyReferences( projectDirectory, "ReferenceAssemblyList" ),
                GetAssemblyReferences( projectDirectory, "AnalyzerAssemblyList" ),
                GetGlobalUsingsFile(),
                GetLicense(),
                GetIgnoredWarnings() );
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