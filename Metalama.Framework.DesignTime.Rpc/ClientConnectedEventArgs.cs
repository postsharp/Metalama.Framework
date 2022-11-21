// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Rpc;

public class ClientConnectedEventArgs : EventArgs
{
    public ProjectKey ProjectKey { get; }

    public ClientConnectedEventArgs( ProjectKey projectKey )
    {
        this.ProjectKey = projectKey;
    }
}