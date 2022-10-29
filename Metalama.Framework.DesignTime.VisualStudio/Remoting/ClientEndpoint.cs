// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Utilities;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ClientEndpoint<T> : ServiceEndpoint, IDisposable
    where T : class
{
    private NamedPipeClientStream? _pipeStream;
    private JsonRpc? _rpc;
    private T? _server;

    protected ClientEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName ) { }

    protected virtual void ConfigureRpc( JsonRpc rpc ) { }

    public async Task ConnectAsync( CancellationToken cancellationToken = default )
    {
        this.Logger.Trace?.Log( $"Connecting to the endpoint '{this.PipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeClientStream( ".", this.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous );
            await this._pipeStream.ConnectAsync( cancellationToken );

            this._rpc = CreateRpc( this._pipeStream );
            this._server = this._rpc.Attach<T>();
            this.ConfigureRpc( this._rpc );
            this._rpc.StartListening();

            this.Logger.Trace?.Log( $"The client is connected to the endpoint '{this.PipeName}'." );

            this.InitializedTask.SetResult( true );
        }
        catch ( Exception e )
        {
            this.Logger.Error?.Log( $"Cannot connect to the endpoint '{this.PipeName}': " + e.Message );

            DesignTimeExceptionHandler.ReportException( e, this.Logger );

            this.InitializedTask.SetException( e );

            throw;
        }
    }

    public async ValueTask<T> GetServerApiAsync( CancellationToken cancellationToken = default )
    {
        await this.WaitUntilInitializedAsync( cancellationToken );

        return this._server ?? throw new InvalidOperationException();
    }

    public void Dispose()
    {
        this._rpc?.Dispose();
        this._pipeStream?.Dispose();
    }
}