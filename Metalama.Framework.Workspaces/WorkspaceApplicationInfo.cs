// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Framework.Workspaces;

internal class WorkspaceApplicationInfo : ApplicationInfoBase
{
    public WorkspaceApplicationInfo() : base( typeof(WorkspaceApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.Workspace";
}