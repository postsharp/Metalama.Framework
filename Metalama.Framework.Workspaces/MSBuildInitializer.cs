// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Microsoft.Build.Locator;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Metalama.Framework.Workspaces;

internal static class MSBuildInitializer
{
    private static readonly ILogger _logger;
    private static VisualStudioInstance? _visualStudioInstance;

    static MSBuildInitializer()
    {
        WorkspaceServices.Initialize();
        _logger = BackstageServiceFactory.ServiceProvider.GetLoggerFactory().GetLogger( "Workspace" );
    }

    public static void Initialize( string projectDirectory )
    {
        if ( !MSBuildLocator.IsRegistered )
        {
            Environment.CurrentDirectory = projectDirectory;
            
            _logger.Trace?.Log(
                $"Initializing MSBuild with directory '{projectDirectory}' with {RuntimeInformation.FrameworkDescription} running on {RuntimeInformation.RuntimeIdentifier}." );

            foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                _logger.Trace?.Log( $"Loaded assembly: '{assembly}' from '{assembly.Location}'." );
            }

            var instances = MSBuildLocator.QueryVisualStudioInstances(
                    new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = projectDirectory } )
                .OrderByDescending( i => i.Version )
                .ToReadOnlyList();

            _logger.Trace?.Log(
                $"Found {instances.Count} instances: {string.Join( ", ", instances.Select( x => $"{x.Name} {x.Version} at '{x.MSBuildPath}'" ) )}" );

            if ( instances.Count == 0 )
            {
                throw new DotNetSdkLoadException(
                    $"Could not find a .NET SDK for {RuntimeInformation.RuntimeIdentifier} {RuntimeInformation.ProcessArchitecture}. Did you select the right .NET version and processor architecture?" );
            }

            _visualStudioInstance = instances.First();

            _logger.Trace?.Log( $"Registering MSBuild instance {_visualStudioInstance.Name} {_visualStudioInstance.Version}." );

            MSBuildLocator.RegisterInstance( _visualStudioInstance );

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;

            /*
            var assemblyLoadContext = AssemblyLoadContext.GetLoadContext( typeof(Workspace).Assembly );

            if ( assemblyLoadContext != null )
            {
                foreach ( var file in Directory.EnumerateFiles( _visualStudioInstance.MSBuildPath, "Microsoft.Build*.dll" ) )
                {
                    _logger.Trace?.Log( $"Loading '{file}' into AssemblyLoadContext." );
                    assemblyLoadContext.LoadFromAssemblyPath( file );
                }
            }*/
        }
    }

    private static void OnFirstChanceException( object? sender, FirstChanceExceptionEventArgs e )
    {
        _logger.Warning?.Log( $"FirstChanceException: {e.Exception}" );
    }

    private static Assembly? OnAssemblyResolve( object? sender, ResolveEventArgs args )
    {
        _logger.Trace?.Log( $"AssemblyResolve: '{args.Name}' requested by '{args.RequestingAssembly}'." );

        return null;
    }

    private static void OnAssemblyLoad( object? sender, AssemblyLoadEventArgs args )
    {
        _logger.Trace?.Log( $"AssemblyLoad: '{args.LoadedAssembly}' from '{args.LoadedAssembly.Location}'." );
    }
}