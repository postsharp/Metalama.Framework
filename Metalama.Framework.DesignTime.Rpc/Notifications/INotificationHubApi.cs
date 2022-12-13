// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;

namespace Metalama.Framework.DesignTime.Rpc.Notifications;

[PublicAPI]
public interface INotificationHubApi : IRpcApi
{
    Task RegisterNotificationListenerAsync( [UsedImplicitly] CancellationToken cancellationToken );

    Task UnregisterNotificationListenerAsync( CancellationToken cancellationToken );
}