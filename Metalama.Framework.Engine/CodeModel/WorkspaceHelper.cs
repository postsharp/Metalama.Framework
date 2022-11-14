// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using System;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// A helper class to work with Roslyn <see cref="Workspace"/>.
/// </summary>
public static class WorkspaceHelper
{
    private static HostServices? _hostServices;

    static WorkspaceHelper()
    {
        CSharpWorkspacesAssembly = LoadRoslynAssembly( "Microsoft.CodeAnalysis.CSharp.Workspaces" );
        CSharpFeaturesAssembly = LoadRoslynAssembly( "Microsoft.CodeAnalysis.CSharp.Features" );
    }

    private static Assembly LoadRoslynAssembly( string name )
    {
        var referencedWorkspaceAssemblyName =
            typeof(OurSyntaxGenerator).Assembly.GetReferencedAssemblies()
                .Single( a => string.Equals( a.Name, "Microsoft.CodeAnalysis.Workspaces", StringComparison.OrdinalIgnoreCase ) );

        var requiredWorkspaceImplementationAssemblyName = new AssemblyName(
            referencedWorkspaceAssemblyName.ToString().ReplaceOrdinal( "Microsoft.CodeAnalysis.Workspaces", name ) );

        // See if the assembly is already loaded in the AppDomain.
        var assembly = AppDomainUtility
            .GetLoadedAssemblies( a => AssemblyName.ReferenceMatchesDefinition( requiredWorkspaceImplementationAssemblyName, a.GetName() ) )
            .OrderByDescending( a => a.GetName().Version )
            .FirstOrDefault();

        if ( assembly != null )
        {
            return assembly;
        }

        // If we must load the assembly, we load the same version as the workspace assembly.
        var workspaceAssembly = typeof(Workspace).Assembly;

        var workspaceImplementationAssemblyName = workspaceAssembly.FullName.Replace(
            workspaceAssembly.GetName().Name,
            name );

        return Assembly.Load( workspaceImplementationAssemblyName );
    }

    public static Assembly CSharpWorkspacesAssembly { get; }

    public static Assembly CSharpFeaturesAssembly { get; }

    /*
    public static HostServices HostServices
    {
        get
        {
            _hostServices ??= MefHostServices.Create(
                new[] { CSharpWorkspacesAssembly, CSharpFeaturesAssembly, typeof(CSharpSyntaxNode).Assembly, typeof(Workspace).Assembly } );

            return _hostServices;
        }
    }
    */

    public static AdhocWorkspace CreateWorkspace()
    {
        return new AdhocWorkspace();
    }
}