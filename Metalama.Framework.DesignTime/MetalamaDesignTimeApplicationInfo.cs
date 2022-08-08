// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.Framework.DesignTime
{
    internal static class DesignTimeServices
    {
        public static void Initialize()
        {
            if ( !MetalamaCompilerInfo.IsActive )
            {
                BackstageServiceFactoryInitializer.Initialize<MetalamaDesignTimeApplicationInfo>();
            }
        }
    }

    internal sealed class MetalamaDesignTimeApplicationInfo : ApplicationInfoBase
    {
        public override ProcessKind ProcessKind { get; }

        public override string Name => "Metalama.DesignTime";

        public override bool IsLongRunningProcess => !MetalamaCompilerInfo.IsActive;

        public bool IsTelemetryEnabled
            =>
#if DEBUG
                false;
#else
            true;
#endif

        public MetalamaDesignTimeApplicationInfo() : base( typeof(MetalamaDesignTimeApplicationInfo).Assembly ) { }
    }
}