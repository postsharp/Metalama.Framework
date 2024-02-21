// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using StreamJsonRpc;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.Rpc;

public abstract class ClientEndpoint<T> : ServiceEndpoint, IDisposable
    where T : class, IRpcApi
{
    private NamedPipeClientStream? _pipeStream;
    private JsonRpc? _rpc;
    private T? _server;
    private volatile int _connecting;

    protected ClientEndpoint( IServiceProvider serviceProvider, string pipeName, JsonSerializationBinder? binder = null ) :
        base( serviceProvider, pipeName, binder ) { }

    protected virtual void ConfigureRpc( JsonRpc rpc ) { }

    protected virtual Task OnConnectedAsync( CancellationToken cancellationToken ) => Task.CompletedTask;

    public async Task<bool> ConnectAsync( CancellationToken cancellationToken = default )
    {
        if ( Interlocked.CompareExchange( ref this._connecting, 1, 0 ) != 0 )
        {
            this.Logger.Trace?.Log( $"The race to connect to the endpoint '{this.PipeName}' was lost." );

            await this.WaitUntilInitializedAsync( nameof(this.ConnectAsync), cancellationToken );

            return false;
        }

        this.Logger.Trace?.Log( $"Connecting to the endpoint '{this.PipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeClientStream( ".", this.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous );
            await this._pipeStream.ConnectAsync( cancellationToken );

            this._rpc = this.CreateRpc( this._pipeStream );
            this._server = this._rpc.Attach<T>();
            this.ConfigureRpc( this._rpc );
            this._rpc.StartListening();

            this.Logger.Trace?.Log( $"The client is connected to the endpoint '{this.PipeName}'." );

            await this.OnConnectedAsync( cancellationToken );

            this.InitializedTask.SetResult( true );

            return true;
        }
        catch ( Exception e )
        {
            this.Logger.Error?.Log( $"Cannot connect to the endpoint '{this.PipeName}': " + e.Message );

            this.ExceptionHandler?.OnException( e, this.Logger );

            this.InitializedTask.SetException( e );

            throw;
        }
    }

    public async ValueTask<T> GetServerApiAsync( string callerName, CancellationToken cancellationToken = default )
    {
        await this.WaitUntilInitializedAsync( callerName, cancellationToken );

        return this._server ?? throw new InvalidOperationException();
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            this._rpc?.Dispose();
            this._pipeStream?.Dispose();
        }
    }

    public void Dispose()
    {
        this.Dispose( true );
    }
}