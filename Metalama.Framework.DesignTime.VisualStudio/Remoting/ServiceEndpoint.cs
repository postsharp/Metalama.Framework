// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using StreamJsonRpc;
using System.Diagnostics;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// A base class for <see cref="UserProcessEndpoint"/> and <see cref="AnalysisProcessEndpoint"/>.
/// </summary>
internal class ServiceEndpoint
{
    protected TaskCompletionSource<bool> InitializedTask { get; } = new();

    protected ILogger Logger { get; }

    protected string PipeName { get; }

    protected ServiceEndpoint( IServiceProvider serviceProvider, string pipeName )
    {
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
        this.PipeName = pipeName;
    }

    public async ValueTask WaitUntilInitializedAsync( string callerName, CancellationToken cancellationToken = default )
    {
        if ( this.InitializedTask.Task.IsCompleted )
        {
            return;
        }

        // Take the stack trace before any yield.
        string stackTrace;

        if ( this.Logger.Warning != null )
        {
            try
            {
                stackTrace = new StackTrace().ToString();
            }
            catch
            {
                stackTrace = "(cannot get a stack trace)";
            }

            var delayTask = Task.Delay( 1000, cancellationToken );

            if ( await Task.WhenAny( delayTask, this.InitializedTask.Task ) == delayTask )
            {
                this.Logger.Warning.Log(
                    $"Waiting for the endpoint '{this.PipeName}' to be initialized because of {callerName} is taking a long time " + Environment.NewLine
                    + stackTrace );
            }

            await this.InitializedTask.Task.WithCancellation( cancellationToken );

            this.Logger.Trace?.Log( $"Endpoint '{this.PipeName}' is now initialized." );
        }
    }

    protected enum ServiceRole
    {
        Discovery,
        Service
    }

    protected static string GetPipeName( ServiceRole role, int? processId = default )
    {
        return $"Metalama_{role.ToString().ToLowerInvariant()}_{processId ?? Process.GetCurrentProcess().Id}_{EngineAssemblyMetadataReader.Instance.BuildId}";
    }

    protected static JsonRpc CreateRpc( Stream stream )
    {
        // MessagePackFormatter does not work in the devenv process, probably because devenv sets it up with some global effect.

        /*
        var formatter = new MessagePackFormatter();
        var options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create( BuiltinResolver.Instance, DynamicObjectResolverAllowPrivate.Instance ) );
            formatter.SetMessagePackSerializerOptions( options );
        */

        var formatter = new JsonMessageFormatter();
        formatter.JsonSerializer.TypeNameHandling = TypeNameHandling.All;

        // We have to specify the full assembly name otherwise there are conflicts when several versions of Metalama are loaded in the AppDomain (see #31075).
        formatter.JsonSerializer.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;

        var handler = new LengthHeaderMessageHandler( stream, stream, formatter );

        return new JsonRpc( handler );
    }
}