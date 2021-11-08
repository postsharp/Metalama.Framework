// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Caravela.Framework.CompilerExtensions
{
    /// <summary>
    /// Extract dependency assemblies packed as managed resources and provides instances of classes implemented by these dependencies.
    /// </summary>
    public static class ResourceExtractor
    {
        private static readonly object _initializeLock = new();
        private static readonly Dictionary<string, Assembly> _embeddedAssemblies = new( StringComparer.OrdinalIgnoreCase );
        private static string? _snapshotDirectory;
        private static volatile bool _initialized;
        private static Assembly? _caravelaImplementationAssembly;

        /// <summary>
        /// Creates an instance of a type of the <c>Caravela.Framework.Impl</c> assembly.
        /// </summary>
        public static object CreateInstance( string name )
        {
            try
            {
                if ( !_initialized )
                {
                    lock ( _initializeLock )
                    {
                        if ( !_initialized )
                        {
                            // To debug, uncomment the next line.
                            // System.Diagnostics.Debugger.Launch();

                            var currentAssembly = typeof(ResourceExtractor).Assembly;

                            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

                            // Get a temp directory. AssemblyName.GetAssemblyName does not support long paths.
                            _snapshotDirectory = TempPathHelper.GetTempPath( "Extract" );

                            // Extract embedded assemblies to a temp directory.
                            ExtractEmbeddedAssemblies( currentAssembly );

                            // Load assemblies from the temp directory.
                            foreach ( var file in Directory.GetFiles( _snapshotDirectory, "*.dll" ) )
                            {
                                var assembly = Assembly.LoadFile( file );
                                _embeddedAssemblies[assembly.GetName().Name] = Assembly.LoadFile( file );
                            }

                            _caravelaImplementationAssembly = _embeddedAssemblies["Caravela.Framework.Impl"];

                            _initialized = true;
                        }
                    }
                }

                var type = _caravelaImplementationAssembly!.GetType( name, true );

                return Activator.CreateInstance( type );
            }
            catch ( Exception e )
            {
                var directory = TempPathHelper.GetTempPath( "ExtractExceptions" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                var path = Path.Combine( directory, Guid.NewGuid().ToString() + ".txt" );

                File.WriteAllText( path, e.ToString() );

                throw;
            }
        }

        private static void ExtractEmbeddedAssemblies( Assembly currentAssembly )
        {
            // Extract managed resources to a snapshot directory.
            var completedFilePath = Path.Combine( _snapshotDirectory, ".completed" );

            if ( !File.Exists( completedFilePath ) )
            {
                StringBuilder log = new();
                log.AppendLine( $"Extracting resources..." );

                var processName = Process.GetCurrentProcess();
                log.AppendLine( $"Process Name: {processName.ProcessName}" );
                log.AppendLine( $"Process Id: {processName.Id}" );
                log.AppendLine( $"Source Assembly Name: '{currentAssembly.FullName}'" );
                log.AppendLine( $"Source Assembly Location: '{currentAssembly.Location}'" );
                log.AppendLine( "Stack trace:" );
                log.AppendLine( new StackTrace().ToString() );
                log.AppendLine( "----" );

                // We cannot use MutexHelper because of dependencies on an embedded assembly.
                using var extractMutex = new Mutex( false, "Global\\Caravela_Extract_" + AssemblyMetadataReader.BuildId );
                extractMutex.WaitOne();

                try
                {
                    if ( !File.Exists( completedFilePath ) )
                    {
                        Directory.CreateDirectory( _snapshotDirectory );

                        foreach ( var resourceName in currentAssembly.GetManifestResourceNames() )
                        {
                            if ( resourceName.EndsWith( ".dll", StringComparison.OrdinalIgnoreCase ) )
                            {
                                log.AppendLine( $"Extracting resource " + resourceName );

                                // Extract the file to disk.
                                using var stream = currentAssembly.GetManifestResourceStream( resourceName )!;
                                var file = Path.Combine( _snapshotDirectory, resourceName );

                                using ( var outputStream = File.Create( file ) )
                                {
                                    stream.CopyTo( outputStream );
                                }

                                // Rename the assembly to the match the assembly name.
                                var assemblyName = AssemblyName.GetAssemblyName( file );
                                var renamedFile = Path.Combine( _snapshotDirectory, assemblyName.Name + ".dll" );

                                RetryHelper.Retry(
                                    () =>
                                    {
                                        if ( File.Exists( renamedFile ) )
                                        {
                                            File.Delete( renamedFile );
                                        }

                                        File.Move( file, renamedFile );
                                    } );
                            }
                            else
                            {
                                log.AppendLine( "Ignoring resource " + resourceName );
                            }
                        }

                        File.WriteAllText( completedFilePath, log.ToString() );
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

            if ( _embeddedAssemblies.TryGetValue( requestedAssemblyName.Name, out var assembly ) )
            {
                var assemblyName = assembly.GetName();

                // We need to explicitly verify the exact version, because AssemblyName.ReferenceMatchesDefinition is too tolerant. 
                if ( AssemblyName.ReferenceMatchesDefinition( requestedAssemblyName, assemblyName ) 
                     && assemblyName.Version == requestedAssemblyName.Version )
                {
                    return assembly;
                }
                else
                {
                    // This is not the expected version.
                }
            }

            return null;
        }
    }
}