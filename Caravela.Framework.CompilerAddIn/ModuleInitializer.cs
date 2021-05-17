// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Caravela.Framework.Impl
{
    public static class ModuleInitializer
    {
        private static readonly object _initializeLock = new();
        private static readonly Dictionary<string, Assembly> _embeddedAssemblies = new( StringComparer.OrdinalIgnoreCase );
        private static string? _snapshotDirectory;
        private static volatile bool _initialized;

        public static object GetImplementationType( string name )
        {
            if ( !_initialized )
            {
                lock ( _initializeLock )
                {
                    if ( !_initialized )
                    {
                        var currentAassembly = Assembly.GetCallingAssembly();

                        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

                        _snapshotDirectory = Path.Combine( Path.GetTempPath(), "Caravela", AssemblyMetadataReader.MainBuildId, "EmbeddedResources" );

                        // Extract embedded assemblies to a temp directory.
                        ExtractEmbeddedAssemblies( currentAassembly );

                        // Load assemblies from the temp directory.
                        foreach ( var file in Directory.GetFiles( _snapshotDirectory, "*.dll" ) )
                        {
                            var assemblyName = RetryHelper.Retry( () => AssemblyName.GetAssemblyName( file ) );
                            var assembly = RetryHelper.Retry( () => Assembly.LoadFile( file ) );
                            _embeddedAssemblies[assemblyName.Name] = assembly;
                        }

                        _initialized = true;
                    }
                }
            }

            var implementationAssembly = _embeddedAssemblies["Caravela.Framework.Impl"];
            var type = implementationAssembly.GetType( name, true );

            return Activator.CreateInstance( type );
        }

        private static void ExtractEmbeddedAssemblies( Assembly currentAassembly )
        {
            // Extract managed resources to a snapshot directory.

            if ( !Directory.Exists( _snapshotDirectory ) )
            {
                // We cannot use MutexHelper because of dependencies on an embedded assembly.
                using var extractMutex = new Mutex( false, "Global\\Caravela_Extract_" + AssemblyMetadataReader.MainBuildId );
                extractMutex.WaitOne();

                try
                {
                    if ( !Directory.Exists( _snapshotDirectory ) )
                    {
                        Directory.CreateDirectory( _snapshotDirectory );

                        foreach ( var resourceName in currentAassembly.GetManifestResourceNames() )
                        {
                            if ( resourceName.EndsWith( ".dll", StringComparison.OrdinalIgnoreCase ) )
                            {
                                // Remove the namespace prefix from the resource name.
                                var fileName = resourceName.Substring( "Caravela.Framework.Impl.Resources.".Length );

                                // Extract the file to disk.
                                using var stream = currentAassembly.GetManifestResourceStream( resourceName )!;
                                var file = Path.Combine( _snapshotDirectory, fileName );

                                using ( var outputStream = File.Create( file ) )
                                {
                                    stream.CopyTo( outputStream );
                                }
                            }
                        }
                    }
                }
                finally
                {
                    extractMutex.ReleaseMutex();
                }
            }
        }

        private static Assembly? OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            var requestedAssemblyName = new AssemblyName( args.Name );

            if ( _embeddedAssemblies.TryGetValue( requestedAssemblyName.Name, out var assembly )
                 && AssemblyName.ReferenceMatchesDefinition( requestedAssemblyName, assembly.GetName() ) )
            {
                return assembly;
            }

            return null;
        }
    }
}