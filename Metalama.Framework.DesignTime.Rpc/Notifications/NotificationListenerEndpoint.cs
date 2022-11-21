// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using StreamJsonRpc;

namespace Metalama.Framework.DesignTime.Rpc.Notifications;

public class NotificationListenerEndpoint : ClientEndpoint<INotificationListenerApi>
{
    private INotificationHubApi? _server;
    private readonly ApiImplementation _listenerApiImplementation;

    public NotificationListenerEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName )
    {
        this._listenerApiImplementation = new ApiImplementation( this );
    }

    protected override void ConfigureRpc( JsonRpc rpc )
    {
        rpc.AddLocalRpcTarget<INotificationListenerApi>( this._listenerApiImplementation, null );
        this._server = rpc.Attach<INotificationHubApi>();
    }

    protected override async Task OnConnectedAsync( CancellationToken cancellationToken )
    {
        this.Logger.Trace?.Log( "Registering for notifications." );
        await this._server!.RegisterNotificationListenerAsync( CancellationToken.None );
        this.Logger.Trace?.Log( "Registering for notifications: completed." );
    }

    public event Action<CompilationResultChangedEventArgs>? CompilationResultChanged;

    public event Action<NotificationEndpointChangedEventArgs>? NotificationEndpointChanged;

    private class ApiImplementation : INotificationListenerApi
    {
        private readonly NotificationListenerEndpoint _parent;

        public ApiImplementation( NotificationListenerEndpoint parent )
        {
            this._parent = parent;
        }

        public Task NotifyNotificationEndpointChangedAsync( NotificationEndpointChangedEventArgs eventArgs, CancellationToken cancellationToken )
        {
            this._parent.NotificationEndpointChanged?.Invoke( eventArgs );

            return Task.CompletedTask;
        }

        public Task NotifyCompilationResultChangedAsync( CompilationResultChangedEventArgs notification, CancellationToken cancellationToken )
        {
            this._parent.CompilationResultChanged?.Invoke( notification );

            return Task.CompletedTask;
        }
    }
}