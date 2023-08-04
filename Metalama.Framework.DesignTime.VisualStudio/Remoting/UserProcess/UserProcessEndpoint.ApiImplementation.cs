// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;

internal sealed partial class UserProcessEndpoint
{
    /// <summary>
    /// Implementation of the <see cref="IUserProcessApi"/> interface. Processes remote requests.
    /// </summary>
    private sealed class ApiImplementation : IUserProcessApi
    {
        private readonly UserProcessEndpoint _parent;

        public ApiImplementation( UserProcessEndpoint parent )
        {
            this._parent = parent;
        }

        public async Task PublishGeneratedCodeAsync(
            ProjectKey projectKey,
            ImmutableDictionary<string, string> sources,
            CancellationToken cancellationToken = default )
        {
            this._parent.Logger.Trace?.Log( $"Received new generated code from the remote host for project '{projectKey}'." );

            // Store the event so that a source generator that would be create later can retrieve it.
            this._parent._cachedGeneratedSources[projectKey] = sources;

            if ( this._parent._projectHandlers.TryGetValue( projectKey, out var client ) )
            {
                await client.PublishGeneratedCodeAsync( projectKey, sources, cancellationToken );
            }
            else
            {
                this._parent.Logger.Warning?.Log( $"No client registered for project '{projectKey}'." );
            }
        }

        public void OnIsEditingCompileTimeCodeChanged( bool isEditing )
        {
            this._parent.IsEditingCompileTimeCodeChanged?.Invoke( isEditing );
        }

        public void OnCompileTimeErrorsChanged( ProjectKey projectKey, IReadOnlyCollection<DiagnosticData> diagnostics )
        {
            this._parent.SetCompileTimeErrors( projectKey, diagnostics );
        }
    }
}