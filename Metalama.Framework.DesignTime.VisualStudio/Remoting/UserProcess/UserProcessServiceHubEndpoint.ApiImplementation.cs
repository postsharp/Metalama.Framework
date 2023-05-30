// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using StreamJsonRpc;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;

internal sealed partial class UserProcessServiceHubEndpoint
{
    private sealed class ApiImplementation : IServiceHubApi
    {
        private readonly UserProcessServiceHubEndpoint _parent;
        private readonly JsonRpc _rpc;

        public INotificationListenerApi? NotificationListener { get; private set; }

        public ApiImplementation( UserProcessServiceHubEndpoint parent, JsonRpc rpc )
        {
            this._parent = parent;
            this._rpc = rpc;
        }

        public async Task RegisterAnalysisServiceAsync( string pipeName, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Registering the endpoint '{pipeName}'." );
            var endpoint = new UserProcessEndpoint( this._parent._serviceProvider, pipeName );
            endpoint.IsEditingCompileTimeCodeChanged += this._parent.OnIsEditingCompileTimeCodeChanged;
            await endpoint.ConnectAsync( cancellationToken );

            if ( this._parent._registeredEndpointsByPipeName.TryAdd( pipeName, endpoint ) )
            {
                this._parent.EndpointAdded?.Invoke( endpoint );
            }
            else
            {
                this._parent.Logger.Error?.Log( $"The endpoint '{pipeName}' was already registered." );
            }
        }

        public Task RegisterAnalysisServiceProjectAsync( ProjectKey projectKey, string pipeName, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Registering the project '{projectKey}' for endpoint '{pipeName}'." );

            if ( this._parent._registeredEndpointsByPipeName.TryGetValue( pipeName, out var endpoint ) )
            {
                if ( !this._parent._registeredEndpointsByProject.TryAdd( projectKey, endpoint ) )
                {
                    this._parent.Logger.Error?.Log( $"The project '{projectKey}' was already registered." );
                }

                // Unblock waiters.
                if ( this._parent._waiters.TryRemove( projectKey, out var waiter ) )
                {
                    waiter.SetResult( endpoint );
                }
            }
            else
            {
                this._parent.Logger.Error?.Log( $"The endpoint '{pipeName}' it not registered." );
            }

            return Task.CompletedTask;
        }

        public Task UnregisterAnalysisServiceAsync( string pipeName, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Unregistering the endpoint '{pipeName}'." );

            if ( this._parent._registeredEndpointsByPipeName.TryRemove( pipeName, out var endpoint ) )
            {
                endpoint.Dispose();
            }
            else
            {
                this._parent.Logger.Error?.Log( $"The endpoint '{pipeName}' is not registered." );
            }

            return Task.CompletedTask;
        }

        public Task UnregisterAnalysisServiceProjectAsync( ProjectKey projectKey, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Unregistering the project '{projectKey}'." );

            if ( !this._parent._registeredEndpointsByProject.TryRemove( projectKey, out _ ) )
            {
                this._parent.Logger.Error?.Log( $"The project '{projectKey}' is not registered." );
            }

            return Task.CompletedTask;
        }

        public Task RegisterNotificationListenerAsync( CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( "Registering a notification listener for the endpoint." );
            this.NotificationListener = this._rpc.Attach<INotificationListenerApi>();

            return Task.CompletedTask;
        }

        public Task UnregisterNotificationListenerAsync( CancellationToken cancellationToken )
        {
            this.NotificationListener = null;

            return Task.CompletedTask;
        }

        public Task NotifyNotificationEndpointChangedAsync( NotificationEndpointChangedEventArgs eventArgs, CancellationToken cancellationToken )
            => Task.CompletedTask;

        public Task NotifyCompilationResultChangedAsync( CompilationResultChangedEventArgs notification, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Received a change notification from client." );

            return this._parent.NotifyCompilationResultChangeAsync( notification, cancellationToken );
        }
    }
}