// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;

namespace Metalama.Framework.DesignTime.Rpc.Notifications;

[JsonObject]
public class NotificationEndpointChangedEventArgs
{
    public Guid ProjectGuid { get; }

    public NotificationEndpointChangedEventArgs( Guid projectGuid )
    {
        this.ProjectGuid = projectGuid;
    }
}