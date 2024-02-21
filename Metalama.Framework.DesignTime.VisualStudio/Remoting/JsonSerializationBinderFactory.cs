// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.DesignTime.Rpc;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal static class JsonSerializationBinderFactory
{
    public static JsonSerializationBinder Instance { get; } = new( new[] { typeof(IAspect).Assembly } );
}