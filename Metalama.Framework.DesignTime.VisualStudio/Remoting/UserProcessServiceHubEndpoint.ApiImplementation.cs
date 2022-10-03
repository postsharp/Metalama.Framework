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
            this._parent.Logger.Trace?.Log( $"Registering the endpoint '{pipeName}'." );
            var endpoint = new UserProcessEndpoint( this._parent._serviceProvider, pipeName );
            endpoint.IsEditingCompileTimeCodeChanged += this._parent.OnIsEditingCompileTimeCodeChanged;
            await endpoint.ConnectAsync( cancellationToken );

            if ( !this._parent._registeredEndpointsByPipeName.TryAdd( pipeName, endpoint ) )
            {
                this._parent.Logger.Error?.Log( $"The endpoint '{pipeName}' was already registered." );
            }
        }

        public Task RegisterProjectAsync( ProjectKey projectKey, string pipeName, CancellationToken cancellationToken )
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

        public Task UnregisterProjectAsync( ProjectKey projectKey, CancellationToken cancellationToken )
        {
            this._parent.Logger.Trace?.Log( $"Registering the project '{projectKey}'." );

            if ( !this._parent._registeredEndpointsByProject.TryRemove( projectKey, out _ ) )
            {
                this._parent.Logger.Error?.Log( $"The project '{projectKey}' is not registered." );
            }

            return Task.CompletedTask;
        }
    }
}