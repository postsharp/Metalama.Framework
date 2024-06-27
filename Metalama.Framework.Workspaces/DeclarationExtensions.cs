// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Introspection;
using Metalama.Framework.Project;
using System.Collections.Generic;

namespace Metalama.Framework.Workspaces;

public static class DeclarationExtensions
{
    /// <summary>
    /// Gets inbound declaration references, i.e. the list of declarations that use the given declaration,
    /// in the projects loaded in the current <see cref="Workspace"/>. 
    /// </summary>
    public static IEnumerable<IDeclarationReference> GetInboundReferences(
        this IDeclaration declaration,
        ReferenceGraphChildKinds childKinds = ReferenceGraphChildKinds.ContainingDeclaration )
    {
        var service = declaration.Compilation.Project.ServiceProvider.GetRequiredService<WorkspaceIntrospectionService>();
        var graph = service.GetReferenceGraph();

        return graph.GetInboundReferences( declaration, childKinds );
    }

    /// <summary>
    /// Get all types derived from a given type within the projects loaded in the current <see cref="Workspace"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="directOnly">If <c>true</c>, only types directly derived from <paramref name="type"/> are returned.</param>
    public static IEnumerable<INamedType> GetDerivedTypes( this INamedType type, bool directOnly = false )
    {
        var service = type.Compilation.Project.ServiceProvider.GetRequiredService<WorkspaceIntrospectionService>();

        return service.GetDerivedTypes( type, directOnly );
    }
}