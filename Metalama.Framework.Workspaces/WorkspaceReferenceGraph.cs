// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Introspection;
using Metalama.Framework.Project;
using System.Collections.Generic;
using System.Diagnostics;

namespace Metalama.Framework.Workspaces;

internal sealed class WorkspaceReferenceGraph : IReferenceGraph
{
    private readonly Future<Workspace> _workspace;

    public WorkspaceReferenceGraph( Future<Workspace> workspace )
    {
        this._workspace = workspace;
    }

    public IReadOnlyCollection<IDeclarationReference> GetIncomingReferences(
        IDeclaration destination,
        ReferenceGraphChildKinds childKinds = ReferenceGraphChildKinds.ContainingDeclaration )
    {
        var result = new List<IDeclarationReference>();

        Debugger.Launch();

        foreach ( var project in this._workspace.Value.Projects )
        {
            var service = project.Compilation.Project.ServiceProvider.GetRequiredService<IProjectIntrospectionService>();
            var graph = service.GetReferenceGraph( project.Compilation );
            result.AddRange( graph.GetIncomingReferences( destination, childKinds ) );
        }

        return result;
    }
}