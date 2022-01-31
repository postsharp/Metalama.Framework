// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal partial class UserProcessEndpoint
{
    /// <summary>
    /// Implementation of the <see cref="IUserProcessApi"/> interface. Processes remote requests.
    /// </summary>
    private class ApiImplementation : IUserProcessApi
    {
        private readonly UserProcessEndpoint _parent;

        public ApiImplementation( UserProcessEndpoint parent )
        {
            this._parent = parent;
        }

        public async Task PublishGeneratedCodeAsync(
            string projectId,
            ImmutableDictionary<string, string> sources,
            CancellationToken cancellationToken = default )
        {
            this._parent._logger.Trace?.Log( $"Received new generated code from the remote host for project '{projectId}'." );

            if ( this._parent._projectHandlers.TryGetValue( projectId, out var client ) )
            {
                await client.PublishGeneratedCodeAsync( projectId, sources, cancellationToken );
            }
            else
            {
                this._parent._logger.Warning?.Log( $"No client registered for project '{projectId}'." );

                // Store the event so that a source generator that would be create later can retrieve it.
                this._parent._unhandledSources[projectId] = sources;
            }
        }

        public void OnIsEditingCompileTimeCodeChanged( bool isEditing )
        {
            this._parent.IsEditingCompileTimeCodeChanged?.Invoke( isEditing );
        }
    }
}