// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// User-side implementation of the <see cref="ICompileTimeEditingStatusService"/> interface.
/// It essentially forwards messages to and from the analysis process.
/// </summary>
internal class CompileTimeEditingStatusService : ICompileTimeEditingStatusService, IDisposable
{
    private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;
    private readonly TaskBag _pendingTasks;
    private bool _userInterfaceAttached;

    public CompileTimeEditingStatusService( GlobalServiceProvider serviceProvider )
    {
        var logger = serviceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
        this._pendingTasks = new TaskBag( logger );
        this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
        this._userProcessEndpoint.EndpointAdded += this.OnEndpointAdded;
        this._userProcessEndpoint.IsEditingCompileTimeCodeChanged += this.OnIsEditingChanged;
    }

    private void OnEndpointAdded( UserProcessEndpoint endpoint )
    {
        this._pendingTasks.Run(
            async () =>
            {
                if ( this._userInterfaceAttached )
                {
                    var api = await endpoint.GetServerApiAsync( nameof(this.OnEndpointAdded) );
                    await api.OnUserInterfaceAttachedAsync();
                }
            } );
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
            var api = await endpoint.GetServerApiAsync( nameof(this.OnEditingCompletedAsync), cancellationToken );
            await api.OnCompileTimeCodeEditingCompletedAsync( cancellationToken );
        }
    }

    public async Task OnUserInterfaceAttachedAsync( CancellationToken cancellationToken )
    {
        this._userInterfaceAttached = true;

        foreach ( var endpoint in this._userProcessEndpoint.Endpoints )
        {
            var api = await endpoint.GetServerApiAsync( nameof(this.OnUserInterfaceAttachedAsync), cancellationToken );
            await api.OnUserInterfaceAttachedAsync( cancellationToken );
        }
    }

    public void Dispose()
    {
        this._userProcessEndpoint.IsEditingCompileTimeCodeChanged -= this.OnIsEditingChanged;
        this._userProcessEndpoint.EndpointAdded -= this.OnEndpointAdded;
    }
}