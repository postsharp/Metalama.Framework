// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.XunitFramework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Reads the set of <see cref="TestAssemblyReference"/> from the project.
    /// </summary>
    internal static class TestAssemblyReferenceReader
    {
        private static readonly ConcurrentDictionary<string, TestAssemblyReference[]> _assemblyReferencesCache = new();

        public static IEnumerable<TestAssemblyReference> GetAssemblyReferences( IAssemblyInfo assembly )
        {
            return _assemblyReferencesCache.GetOrAdd( assembly.AssemblyPath, _ => GetAssembliesReferencesCore( assembly ) );
        }

        private static TestAssemblyReference[] GetAssembliesReferencesCore( IAssemblyInfo assembly )
        {
            var attribute = assembly
                .GetCustomAttributes( typeof(AssemblyMetadataAttribute) )
                .SingleOrDefault( a => (string) a.GetConstructorArguments().First<object>() == "ReferenceAssemblyList" );

            if ( attribute == null )
            {
                throw new InvalidOperationException( "The test assembly must have a single AssemblyMetadataAttribute with Key = \"ReferenceAssemblyList\"." );
            }

            TestDiscoverer testDiscoverer = new( assembly );
            var projectDirectory = testDiscoverer.FindProjectDirectory();

            var path = (string) attribute.GetConstructorArguments().ElementAt( 1 )!;

            var lines = File.ReadAllLines( path );

            return lines.Select( t => new TestAssemblyReference { Path = FilterReference( projectDirectory, t ) } ).ToArray();
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