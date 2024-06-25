// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Introspection;
using Metalama.Framework.Project;
using System.Collections.Generic;

namespace Metalama.Framework.Workspaces;

public static class DeclarationExtensions
{
    /// <summary>
    /// Gets
    /// </summary>
    /// <param name="declaration"></param>
    /// <param name="childKinds"></param>
    /// <returns></returns>
    public static IEnumerable<IDeclarationReference> GetIncomingReferences(
        this IDeclaration declaration,
        ReferenceGraphChildKinds childKinds = ReferenceGraphChildKinds.ContainingDeclaration )
    {
        var graph = declaration.Compilation.Project.ServiceProvider.GetRequiredService<WorkspaceIntrospectionService>().GetReferenceGraph();

        return graph.GetIncomingReferences( declaration, childKinds );
    }
}