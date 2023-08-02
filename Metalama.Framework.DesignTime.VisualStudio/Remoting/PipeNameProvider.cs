// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using System.Diagnostics;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal static class PipeNameProvider
{
    public static string GetPipeName( ServiceRole role, int? processId = default )
    {
        // The hash must be the across all target frameworks.
        var buildHash = HashUtilities.HashString( EngineAssemblyMetadataReader.Instance.PackageVersion.AssertNotNull() );

        return $"Metalama_{role.ToString().ToLowerInvariant()}_{processId ?? Process.GetCurrentProcess().Id}_{buildHash}";
    }
}