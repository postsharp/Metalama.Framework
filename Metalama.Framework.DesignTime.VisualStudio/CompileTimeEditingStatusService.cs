// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Project;
using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.VisualStudio;

internal class CompileTimeEditingStatusService : ICompileTimeEditingStatusService, IDisposable
{
    private readonly ServiceClient _serviceClient;
    private readonly ConcurrentBag<ICompileTimeEditingStatusServiceCallback> _callbacks = new();

    public CompileTimeEditingStatusService( IServiceProvider serviceProvider )
    {
        this._serviceClient = serviceProvider.GetRequiredService<ServiceClient>();
        this._serviceClient.IsEditingCompileTimeCodeChanged += this.OnIsEditingChanged;
    }

    private void OnIsEditingChanged( bool value )
    {
        this.IsEditing = value;

        foreach ( var callback in this._callbacks )
        {
            callback.OnIsEditingChanged( value );
        }
    }

    public bool IsEditing { get; private set; }

    public void RegisterCallback( ICompileTimeEditingStatusServiceCallback callback ) => this._callbacks.Add( callback );

    public async Task OnEditingCompletedAsync( CancellationToken cancellationToken )
    {
        var api = await this._serviceClient.GetServerApiAsync( cancellationToken );
        await api.OnCompileTimeCodeEditingCompletedAsync( cancellationToken );
    }

    public void Dispose()
    {
        this._serviceClient.IsEditingCompileTimeCodeChanged -= this.OnIsEditingChanged;
    }
}