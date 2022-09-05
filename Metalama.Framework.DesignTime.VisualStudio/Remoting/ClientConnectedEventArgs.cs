// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ClientConnectedEventArgs : EventArgs
{
    public string ProjectId { get; }

    public ClientConnectedEventArgs( string projectId )
    {
        this.ProjectId = projectId;
    }
}