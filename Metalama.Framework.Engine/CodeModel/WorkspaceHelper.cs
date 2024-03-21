// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// A helper class to work with Roslyn <see cref="Workspace"/>.
/// </summary>
internal static class WorkspaceHelper
{
    static WorkspaceHelper()
    {
        CSharpWorkspacesAssembly = LoadRoslynAssembly( "Microsoft.CodeAnalysis.CSharp.Workspaces" );
    }

    private static Assembly LoadRoslynAssembly( string name )
    {
        var referencedWorkspaceAssemblyName =
            typeof(ContextualSyntaxGenerator).Assembly.GetReferencedAssemblies()
                .Single( a => string.Equals( a.Name, "Microsoft.CodeAnalysis.Workspaces", StringComparison.OrdinalIgnoreCase ) );

        var requiredWorkspaceImplementationAssemblyName = new AssemblyName(
            referencedWorkspaceAssemblyName.ToString().ReplaceOrdinal( "Microsoft.CodeAnalysis.Workspaces", name ) );

        // See if the assembly is already loaded in the AppDomain.
        var assembly = AppDomainUtility
            .GetLoadedAssemblies( a => AssemblyName.ReferenceMatchesDefinition( requiredWorkspaceImplementationAssemblyName, a.GetName() ) )
            .MaxByOrNull( a => a.GetName().Version );

        if ( assembly != null )
        {
            return assembly;
        }

        // If we must load the assembly, we load the same version as the workspace assembly.
        var workspaceAssembly = typeof(Workspace).Assembly;

        // ReSharper disable once RedundantSuppressNullableWarningExpression
        var workspaceImplementationAssemblyName = workspaceAssembly.FullName!.ReplaceOrdinal(
            workspaceAssembly.GetName().Name!,
            name );

        return Assembly.Load( workspaceImplementationAssemblyName );
    }

    public static Assembly CSharpWorkspacesAssembly { get; }
}