// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Metalama.Framework.CompilerExtensions
{
    /// <summary>
    /// Extract dependency assemblies packed as managed resources and provides instances of classes implemented by these dependencies.
    /// </summary>
    public static class ResourceExtractor
    {
        private static readonly object _initializeLock = new();
        private static readonly Dictionary<string, ( string Path, AssemblyName Name )> _embeddedAssemblies = new( StringComparer.OrdinalIgnoreCase );

        private static readonly ConcurrentDictionary<string, Assembly?> _assemblyCache = new( StringComparer.OrdinalIgnoreCase );

        private static string? _snapshotDirectory;
        private static volatile bool _initialized;

        private static void Initialize()
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
                            // We don't load assemblies using Assembly.LoadFile here, because the assemblies may be loaded in
                            // the main load context, or may be loaded later. We will use Assembly.LoadFile in last chance in the AssemblyResolve event.
                            // This scenario is used in Metalama.Try.

                            var loadedAssemblyName = AssemblyName.GetAssemblyName( file );
                            _embeddedAssemblies[loadedAssemblyName.Name] = (file, loadedAssemblyName);
                        }

                        _initialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an instance of a type of the <c>Metalama.Framework.Engine</c> assembly.
        /// </summary>
        public static object CreateInstance( string assemblyName, string typeName )
        {
            try
            {
                Initialize();

                var assembly = GetAssembly( assemblyName );

                if ( assembly == null )
                {
                    throw new ArgumentOutOfRangeException( $"Cannot load the assembly '{assemblyName}'" );
                }

                var type = assembly.GetType( typeName );

                if ( type == null )
                {
                    throw new ArgumentOutOfRangeException( $"Cannot load the type '{typeName}' in assembly '{assemblyName}'" );
                }

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
                log.AppendLine( $"Process Kind: {ProcessKindHelper.CurrentProcessKind}" );
                log.AppendLine( $"Command Line: {Environment.CommandLine}" );
                log.AppendLine( $"Source Assembly Name: '{currentAssembly.FullName}'" );
                log.AppendLine( $"Source Assembly Location: '{currentAssembly.Location}'" );
                log.AppendLine( "Stack trace:" );
                log.AppendLine( new StackTrace().ToString() );
                log.AppendLine( "----" );

                // We cannot use MutexHelper because of dependencies on an embedded assembly.
                using var extractMutex = new Mutex( false, "Global\\Metalama_Extract_" + AssemblyMetadataReader.BuildId );
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

        private static Assembly? GetAssembly( string name )
        {
            return _assemblyCache.GetOrAdd( name, Load );

            static Assembly? Load( string name )
            {
                var requestedAssemblyName = new AssemblyName( name );

                bool CandidateMatchesRequest( AssemblyName candidate )
                    => AssemblyName.ReferenceMatchesDefinition( requestedAssemblyName, candidate )
                       && (requestedAssemblyName.Version == null || candidate.Version == requestedAssemblyName.Version);

                // First try to find the assembly in the AppDomain.
                var existingAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault( x => CandidateMatchesRequest( x.GetName() ) );

                if ( existingAssembly != null )
                {
                    return existingAssembly;
                }

                // Find for an assembly in the current AppDomain.

                if ( _embeddedAssemblies.TryGetValue( requestedAssemblyName.Name, out var assembly ) )
                {
                    var assemblyName = assembly.Name;

                    // We need to explicitly verify the exact version, because AssemblyName.ReferenceMatchesDefinition is too tolerant. 
                    if ( CandidateMatchesRequest( assemblyName ) )
                    {
                        return Assembly.LoadFile( assembly.Path );
                    }
                    else
                    {
                        // This is not the expected version.
                    }
                }

                return null;
            }
        }

        private static Assembly? OnAssemblyResolve( object sender, ResolveEventArgs args ) => GetAssembly( args.Name );
    }
}