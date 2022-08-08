using Metalama.Backstage.Extensibility;

namespace Metalama.Framework.Workspaces;

internal class WorkspaceApplicationInfo : ApplicationInfoBase
{
    public WorkspaceApplicationInfo() : base( typeof(WorkspaceApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.Workspace";
}