// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.LinqPad;

internal readonly struct GetCompilationInfo
{
    public string WorkspaceExpression { get; }

    public bool IsMetalamaOutput { get; }

    public GetCompilationInfo( string workspaceExpression, bool isMetalamaOutput )
    {
        this.WorkspaceExpression = workspaceExpression;
        this.IsMetalamaOutput = isMetalamaOutput;
    }
}