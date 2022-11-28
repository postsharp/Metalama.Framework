// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private static readonly Dictionary<string, (string Path, AssemblyName Name)> _embeddedAssemblies = new( StringComparer.OrdinalIgnoreCase );

        private static readonly ConcurrentDictionary<string, Assembly?> _assemblyCache = new( StringComparer.OrdinalIgnoreCase );

        private static readonly string _snapshotDirectory;
        private static readonly string _buildId;
        private static readonly PropertyInfo? _isCollectibleProperty = typeof(Assembly).GetProperty( "IsCollectible" );
        private static volatile bool _initialized;
        private static string? _versionNumber;

        static ResourceExtractor()
        {
            if ( !string.IsNullOrEmpty( Environment.GetEnvironmentVariable( "METALAMA_DEBUG_RESOURCE_EXTRACTOR" ) ) )
            {
                Debugger.Launch();
            }

            // This mimics the logic implemented by TempPathHelper and backed by Metalama.Backstage, however without having a reference to Metalama.Backstage.
            var assembly = typeof(ResourceExtractor).Assembly;
            var moduleId = assembly.ManifestModule.ModuleVersionId;
            var assemblyVersion = assembly.GetName().Version;

            _buildId = assemblyVersion.ToString( 4 ) + "-" +
                       string.Join( "", moduleId.ToByteArray().Take( 4 ).Select( i => i.ToString( "x2", CultureInfo.InvariantCulture ) ) );

            _snapshotDirectory = GetTempDirectory( "Extract" );
        }

        private static string GetTempDirectory( string purpose ) => Path.Combine( Path.GetTempPath(), "Metalama", purpose, _buildId );

        // .NET 5.0 has collectible assemblies, but collectible assemblies cannot be returned to AppDomain.AssemblyResolve.
        private static bool IsCollectible( Assembly assembly ) => _isCollectibleProperty == null || (bool) _isCollectibleProperty.GetValue( assembly );

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

                        _versionNumber = GetRoslynVersion();

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

                assemblyName = assemblyName + "." + _versionNumber;

                var assemblyQualifiedName = _embeddedAssemblies[assemblyName].Name.ToString();

                var assembly = GetAssembly( assemblyQualifiedName );

                if ( assembly == null )
                {
                    throw new ArgumentOutOfRangeException( nameof(assemblyName), $"Cannot load the assembly '{assemblyQualifiedName}'" );
                }

                var type = assembly.GetType( typeName, true );

                if ( type == null )
                {
                    throw new ArgumentOutOfRangeException( nameof(typeName), $"Cannot load the type '{typeName}' in assembly '{assemblyQualifiedName}'" );
                }

                return Activator.CreateInstance( type );
            }
            catch ( Exception e )
            {
                var directory = GetTempDirectory( "ExtractExceptions" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );

                    // Mark the directory for automatic clean up when unused.
                    var cleanupJsonFilePath = Path.Combine( directory, "cleanup.json" );
                    File.WriteAllText( cleanupJsonFilePath, "{\"Strategy\":1}" );
                }

                var path = Path.Combine( directory, Guid.NewGuid().ToString() + ".txt" );

                var exceptionReport = new StringBuilder();
                var process = Process.GetCurrentProcess();
                exceptionReport.AppendLine( $"Process Name: {process.ProcessName}" );
                exceptionReport.AppendLine( $"Process Id: {process.Id}" );
                exceptionReport.AppendLine( $"Process Kind: {ProcessKindHelper.CurrentProcessKind}" );
                exceptionReport.AppendLine( $"Command Line: {Environment.CommandLine}" );
                exceptionReport.AppendLine();
                exceptionReport.AppendLine( e.ToString() );

                File.WriteAllText( path, exceptionReport.ToString() );

                throw;
            }
        }

        private static void ExtractEmbeddedAssemblies( Assembly currentAssembly )
        {
            // Extract managed resources to a snapshot directory.
            var completedFilePath = Path.Combine( _snapshotDirectory, ".completed" );
            var cleanupJsonFilePath = Path.Combine( _snapshotDirectory, "cleanup.json" );
            var mutexName = "Global\\Metalama_Extract_" + _buildId;

            if ( !File.Exists( completedFilePath ) )
            {
                // We cannot use MutexHelper because of dependencies on an embedded assembly.

                using var extractMutex = new Mutex( false, mutexName );

                try
                {
                    extractMutex.WaitOne();
                }
                catch ( AbandonedMutexException )
                {
                    // Another process crashed while holding the mutex.
                    // This situation can be ignored because the presence of the `.completed` file alone says
                    // that the extraction was successful.
                }

                StreamWriter? log = null;

                try
                {
                    if ( !File.Exists( completedFilePath ) )
                    {
                        if ( !Directory.Exists( _snapshotDirectory ) )
                        {
                            Directory.CreateDirectory( _snapshotDirectory );

                            // Mark the directory for automatic clean up when unused.
                            File.WriteAllText( cleanupJsonFilePath, "{\"Strategy\":2}" );
                        }

                        log = File.CreateText( Path.Combine( _snapshotDirectory, $"extract-{Guid.NewGuid()}.log" ) );

                        log.WriteLine( $"Extracting resources..." );

                        var process = Process.GetCurrentProcess();
                        log.WriteLine( $"Process Name: {process.ProcessName}" );
                        log.WriteLine( $"Process Id: {process.Id}" );
                        log.WriteLine( $"Process Kind: {ProcessKindHelper.CurrentProcessKind}" );
                        log.WriteLine( $"Command Line: {Environment.CommandLine}" );
                        log.WriteLine( $"Source Assembly Name: '{currentAssembly.FullName}'" );
                        log.WriteLine( $"Source Assembly Location: '{currentAssembly.Location}'" );
                        log.WriteLine( $"Mutex name: '{mutexName}'" );
                        log.WriteLine( "----" );

                        foreach ( var resourceName in currentAssembly.GetManifestResourceNames() )
                        {
                            var prefix = "Metalama.Framework.CompilerExtensions.Resources.";

                            if ( resourceName.EndsWith( ".dll", StringComparison.OrdinalIgnoreCase ) &&
                                 resourceName.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) )
                            {
                                var fileName = resourceName.Substring( prefix.Length );
                                var filePath = Path.Combine( _snapshotDirectory, fileName );

                                log.WriteLine( $"Extracting resource '{resourceName}' to '{filePath}'." );

                                // Extract the file to disk.
                                using var stream = currentAssembly.GetManifestResourceStream( resourceName )!;

                                using ( var outputStream = File.Create( filePath ) )
                                {
                                    stream.CopyTo( outputStream );
                                }
                            }
                            else
                            {
                                log.WriteLine( $"Ignoring resource '{resourceName}'." );
                            }
                        }

                        File.WriteAllText( completedFilePath, "completed" );
                    }
                }
                catch ( Exception e )
                {
                    log.WriteLine( e.ToString() );

                    throw;
                }
                finally
                {
                    log?.Dispose();
                    extractMutex.ReleaseMutex();
                }
            }
            else if ( File.GetLastWriteTime( cleanupJsonFilePath ) < DateTime.Now.AddHours( -1 ) )
            {
                // Touch the cleanup.json file so the periodic cleanup script does not remove it.

                try
                {
                    File.SetLastAccessTime( cleanupJsonFilePath, DateTime.Now );
                }
                catch { }
            }
        }

        private static Assembly? GetAssembly( string name )
        {
            return _assemblyCache.GetOrAdd( name, Load );

            static Assembly? Load( string name )
            {
                var requestedAssemblyName = new AssemblyName( name );

                // Find for an assembly in the current AppDomain.

                if ( _embeddedAssemblies.TryGetValue( requestedAssemblyName.Name, out var embeddedAssembly ) )
                {
                    var assemblyName = embeddedAssembly.Name;

                    if ( embeddedAssembly.Name.Version == assemblyName.Version )
                    {
                        return Assembly.LoadFile( embeddedAssembly.Path );
                    }
                    else
                    {
                        // This is not the expected version.
                        // Another assembly version should handle it.
                        return null;
                    }
                }
                else
                {
                    // We may get here because one of our assemblies is requesting a lower version of Roslyn
                    // assemblies than what we have. In this case, we will return any matching assembly.

                    bool VersionsMatch( AssemblyName candidate ) => AssemblyName.ReferenceMatchesDefinition( requestedAssemblyName, candidate );

                    var existingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault( x => !IsCollectible( x ) && VersionsMatch( x.GetName() ) );

                    return existingAssembly;
                }
            }
        }

        private static Assembly? OnAssemblyResolve( object sender, ResolveEventArgs args ) => GetAssembly( args.Name );

        private static string GetRoslynVersion()
        {
            var assembly = typeof(SyntaxNode).Assembly;
            var version = assembly.GetName().Version;

            if ( version == new Version( 42, 42, 42, 42 ) )
            {
                // This is the JetBrains build. The real version is in AssemblyInformationalVersionAttribute.

                var informationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

                if ( informationalVersionAttribute != null )
                {
                    var informationalVersionString = informationalVersionAttribute.InformationalVersion.Split( '-' );

                    if ( Version.TryParse( informationalVersionString[0], out var informationVersion ) )
                    {
                        version = informationVersion;
                    }
                }
            }

            if ( version >= new Version( 4, 0 ) )
            {
                return "4.4.0";
            }
            else
            {
                return "4.0.1";
            }
        }
    }
}