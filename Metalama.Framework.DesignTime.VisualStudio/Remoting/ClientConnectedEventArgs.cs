// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ClientConnectedEventArgs : EventArgs
{
    public string ProjectId { get; }

    public ClientConnectedEventArgs( string projectId )
    {
        this.ProjectId = projectId;
    }
}