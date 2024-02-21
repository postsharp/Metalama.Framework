// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using StreamJsonRpc;
using System.Collections.Concurrent;
using System.IO.Pipes;

namespace Metalama.Framework.DesignTime.Rpc;

public abstract class ServerEndpoint : ServiceEndpoint, IDisposable
{
    private readonly int _maxClientCount;
    private readonly CancellationTokenSource _startCancellationSource = new();
    private readonly ConcurrentDictionary<JsonRpc, NamedPipeServerStream> _pipes = new();

    protected ServerEndpoint( IServiceProvider serviceProvider, string pipeName, int maxClientCount, JsonSerializationBinder? binder = null ) : base(
        serviceProvider,
        pipeName,
        binder )
    {
        this._maxClientCount = maxClientCount;
    }

    internal int ClientCount => this._pipes.Count;

#pragma warning disable VSTHRD100 // Avoid "async void".
    /// <summary>
    /// Starts the RPC connection, but does not wait until the service is fully started.
    /// </summary>
    public async void Start()
    {
        try
        {
            await Task.Run( () => this.StartAsync( this._startCancellationSource.Token ) );
        }
        catch ( Exception e )
        {
            this.ExceptionHandler?.OnException( e, this.Logger );
        }
    }
#pragma warning restore VSTHRD100 // Avoid "async void".

    protected abstract void ConfigureRpc( JsonRpc rpc );

    protected virtual Task OnServerPipeCreatedAsync( CancellationToken cancellationToken ) => Task.CompletedTask;

    private async Task StartAsync( CancellationToken cancellationToken )
    {
        this.Logger.Trace?.Log( $"Starting the server endpoint '{this.PipeName}'." );

        try
        {
            await this.AcceptNewClientAsync( cancellationToken );

            this.Logger.Trace?.Log( $"The server endpoint '{this.PipeName}' is ready." );

            this.InitializedTask.SetResult( true );
        }
        catch ( Exception e )
        {
            this.InitializedTask.SetException( e );
            this.ExceptionHandler?.OnException( e, this.Logger );

            throw;
        }
    }

    private async Task AcceptNewClientAsync( CancellationToken cancellationToken )
    {
        var pipe = new NamedPipeServerStream(
            this.PipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous );

        await this.OnServerPipeCreatedAsync( cancellationToken );

        this.Logger.Trace?.Log( $"Endpoint '{this.PipeName}': wait for a client." );

        await pipe.WaitForConnectionAsync( cancellationToken );

        this.Logger.Trace?.Log( $"Endpoint '{this.PipeName}': got a client." );

        var rpc = this.CreateRpc( pipe );
        this.ConfigureRpc( rpc );

        rpc.Disconnected += this.OnRpcDisconnected;
        this._pipes.TryAdd( rpc, pipe );

        this.Logger.Trace?.Log( $"Endpoint '{this.PipeName}': start listening." );
        rpc.StartListening();

        this.Logger.Trace?.Log( $"The server endpoint '{this.PipeName}' is ready." );

        // Listen to another client.
        if ( this.ClientCount < this._maxClientCount )
        {
            _ = Task.Run( () => this.AcceptNewClientAsync( cancellationToken ), cancellationToken );
        }
    }

    private void OnRpcDisconnected( object? sender, JsonRpcDisconnectedEventArgs e )
    {
        this.Logger.Trace?.Log( $"Endpoint '{this.PipeName}': a client got disconnected." );

        this.OnRpcDisconnected( (JsonRpc) sender! );
    }

    protected virtual void OnRpcDisconnected( JsonRpc rpc )
    {
        if ( this._pipes.TryRemove( rpc, out var pipe ) )
        {
            pipe.Dispose();
        }

        // Listen to another client.
        if ( this.ClientCount < this._maxClientCount )
        {
            _ = Task.Run( () => this.AcceptNewClientAsync( this._startCancellationSource.Token ), this._startCancellationSource.Token );
        }
    }

    public virtual void Dispose()
    {
        this.Logger.Trace?.Log( $"Disposing endpoint '{this.PipeName}'." );

        try
        {
            this._startCancellationSource.Cancel();
        }
        catch ( Exception e )
        {
            this.Logger.Error?.Log( e.ToString() );
        }

        foreach ( var pipe in this._pipes )
        {
            try
            {
                if ( !pipe.Key.IsDisposed )
                {
                    pipe.Key.Dispose();
                }

                pipe.Value.Dispose();
            }
            catch ( Exception e )
            {
                this.ExceptionHandler?.OnException( e, this.Logger );
            }
        }
    }
}