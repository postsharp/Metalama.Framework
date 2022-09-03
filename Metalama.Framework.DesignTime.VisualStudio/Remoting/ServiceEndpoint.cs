// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Utilities;
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
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "Remoting" );
        this.PipeName = pipeName;
    }

    public Task WhenInitialized => this.InitializedTask.Task;

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