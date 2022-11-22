// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using System.Diagnostics;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal static class PipeNameProvider
{
    public static string GetPipeName( ServiceRole role, int? processId = default )
    {
        return $"Metalama_{role.ToString().ToLowerInvariant()}_{processId ?? Process.GetCurrentProcess().Id}_{EngineAssemblyMetadataReader.Instance.BuildId}";
    }
}