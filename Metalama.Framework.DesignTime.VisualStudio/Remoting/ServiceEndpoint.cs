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

    protected async ValueTask WaitUntilInitializedAsync( CancellationToken cancellationToken = default )
    {
        if ( this.InitializedTask.Task.IsCompleted )
        {
            return;
        }

        this.Logger.Trace?.Log( $"Waiting for the endpoint '{this.PipeName}' to be initialized." );

        await this.InitializedTask.Task.WithCancellation( cancellationToken );
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
        formatter.JsonSerializer.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;

        var handler = new LengthHeaderMessageHandler( stream, stream, formatter );

        return new JsonRpc( handler );
    }
}