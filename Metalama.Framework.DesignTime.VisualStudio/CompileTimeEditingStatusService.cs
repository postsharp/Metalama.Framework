// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// User-side implementation of the <see cref="ICompileTimeEditingStatusService"/> interface.
/// It essentially forwards messages to and from the analysis process.
/// </summary>
internal class CompileTimeEditingStatusService : ICompileTimeEditingStatusService, IDisposable
{
    private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;

    public CompileTimeEditingStatusService( IServiceProvider serviceProvider )
    {
        this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
        this._userProcessEndpoint.IsEditingCompileTimeCodeChanged += this.OnIsEditingChanged;
    }

    private void OnIsEditingChanged( bool value )
    {
        this.IsEditing = value;
        this.IsEditingChanged?.Invoke( value );
    }

    public bool IsEditing { get; private set; }

    public event Action<bool>? IsEditingChanged;

    public async Task OnEditingCompletedAsync( CancellationToken cancellationToken )
    {
        foreach ( var endpoint in this._userProcessEndpoint.Endpoints )
        {
            var api = await endpoint.GetServerApiAsync( cancellationToken );
            await api.OnCompileTimeCodeEditingCompletedAsync( cancellationToken );
        }
    }

    public async Task OnUserInterfaceAttachedAsync( CancellationToken cancellationToken )
    {
        foreach ( var endpoint in this._userProcessEndpoint.Endpoints )
        {
            var api = await endpoint.GetServerApiAsync( cancellationToken );
            await api.OnUserInterfaceAttachedAsync( cancellationToken );
        }
    }

    public void Dispose()
    {
        this._userProcessEndpoint.IsEditingCompileTimeCodeChanged -= this.OnIsEditingChanged;
    }
}