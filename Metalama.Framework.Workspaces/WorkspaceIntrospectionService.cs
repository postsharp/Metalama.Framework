// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Introspection;
using Metalama.Framework.Services;

namespace Metalama.Framework.Workspaces;

internal class WorkspaceIntrospectionService( Future<Workspace> workspace ) : IProjectService
{
    private readonly WorkspaceReferenceGraph _referenceGraph = new( workspace );

    public IReferenceGraph GetReferenceGraph() => this._referenceGraph;
}