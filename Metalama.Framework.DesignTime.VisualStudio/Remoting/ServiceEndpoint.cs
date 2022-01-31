// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using MessagePack;
using MessagePack.ImmutableCollection;
using MessagePack.Resolvers;
using Newtonsoft.Json;
using StreamJsonRpc;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class ServiceEndpoint
{
    protected static JsonRpc CreateRpc( Stream stream )
    {
        /*
        var formatter = new MessagePackFormatter();
        var options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create( BuiltinResolver.Instance, DynamicObjectResolverAllowPrivate.Instance ) );
            formatter.SetMessagePackSerializerOptions( options );
        */
        
        var formatter = new JsonMessageFormatter();
        formatter.JsonSerializer.TypeNameHandling = TypeNameHandling.All; 
        var handler = new LengthHeaderMessageHandler( stream, stream, formatter );

        return new JsonRpc( handler );
    }
}

