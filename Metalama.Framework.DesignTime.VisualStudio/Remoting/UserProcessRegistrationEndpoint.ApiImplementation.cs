// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal partial class UserProcessServiceHubEndpoint
{
    private class ApiImplementation : IServiceHubApi
    {
        private readonly UserProcessServiceHubEndpoint _parent;

        public ApiImplementation( UserProcessServiceHubEndpoint parent )
        {
            this._parent = parent;
        }

        public async Task RegisterEndpointAsync( string pipeName, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Registering the endpoint '{pipeName}' for project ''.." );
            var endpoint = new UserProcessEndpoint( this._parent._serviceProvider, pipeName );
            endpoint.IsEditingCompileTimeCodeChanged += this._parent.OnIsEditingCompileTimeCodeChanged;
            await endpoint.ConnectAsync( cancellationToken );

            if ( !this._parent._registeredEndpointsByPipeName.TryAdd( pipeName, endpoint ) )
            {
                this._parent.Logger.Error?.Log( $"The endpoint '{pipeName}' was already registered." );
            }
        }

        public Task RegisterProjectAsync( string projectId, string pipeName, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Registering the project '{projectId}' for endpoint '{pipeName}'." );

            if ( this._parent._registeredEndpointsByPipeName.TryGetValue( pipeName, out var endpoint ) )
            {
                if ( !this._parent._registeredEndpointsByProjectId.TryAdd( projectId, endpoint ) )
                {
                    this._parent.Logger.Error?.Log( $"The project '{projectId}' was already registered." );
                }

                // Unblock waiters.
                if ( this._parent._waiters.TryRemove( projectId, out var waiter ) )
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

        public Task UnregisterEndpointAsync( string pipeName, CancellationToken cancellationToken )
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

        public Task UnregisterProjectAsync( string projectId, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Registering the project '{projectId}'." );

            if ( !this._parent._registeredEndpointsByProjectId.TryRemove( projectId, out _ ) )
            {
                this._parent.Logger.Error?.Log( $"The project '{projectId}' is not registered." );
            }

            return Task.CompletedTask;
        }
    }
}