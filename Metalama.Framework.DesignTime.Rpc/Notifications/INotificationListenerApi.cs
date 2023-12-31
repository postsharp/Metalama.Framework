﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;

namespace Metalama.Framework.DesignTime.Rpc.Notifications;

[PublicAPI]
public interface INotificationListenerApi : IRpcApi
{
    Task NotifyNotificationEndpointChangedAsync( NotificationEndpointChangedEventArgs eventArgs, CancellationToken cancellationToken );

    Task NotifyCompilationResultChangedAsync( CompilationResultChangedEventArgs eventArgs, CancellationToken cancellationToken );
}