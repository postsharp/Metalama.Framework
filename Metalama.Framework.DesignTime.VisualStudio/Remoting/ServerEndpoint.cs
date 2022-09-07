// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using StreamJsonRpc;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal abstract class ServerEndpoint : ServiceEndpoint, IDisposable
{
    private readonly CancellationTokenSource _startCancellationSource = new();
    private NamedPipeServerStream? _pipeStream;
    private JsonRpc? _rpc;

    protected ServerEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName ) { }

    /// <summary>
    /// Starts the RPC connection, but does not wait until the service is fully started.
    /// </summary>
    public void Start()
    {
        _ = Task.Run( () => this.StartAsync( this._startCancellationSource.Token ) );
    }

    protected abstract void ConfigureRpc( JsonRpc rpc );

    protected virtual Task OnPipeCreatedAsync( CancellationToken cancellationToken ) => Task.CompletedTask;

    private async Task StartAsync( CancellationToken cancellationToken = default )
    {
        this.Logger.Trace?.Log( $"Starting the endpoint '{this.PipeName}'." );

        try
        {
            this._pipeStream = new NamedPipeServerStream(
                this.PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous );

            await this.OnPipeCreatedAsync( cancellationToken );

            await this._pipeStream.WaitForConnectionAsync( cancellationToken );

            this._rpc = CreateRpc( this._pipeStream );

            this.ConfigureRpc( this._rpc );

            this._rpc.StartListening();

            this.Logger.Trace?.Log( $"The endpoint '{this.PipeName}' is ready." );

            this.InitializedTask.SetResult( true );
        }
        catch ( Exception e )
        {
            this.InitializedTask.SetException( e );
            this.Logger.Error?.Log( "Cannot start the endpoint: " + e );

            throw;
        }
    }

    public virtual void Dispose()
    {
        this._rpc?.Dispose();
        this._pipeStream?.Dispose();
    }
}