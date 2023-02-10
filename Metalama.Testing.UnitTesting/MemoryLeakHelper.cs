// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NET6_0_OR_GREATER || NETFRAMEWORK

using JetBrains.Profiler.SelfApi;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine;
using System.IO;

namespace Metalama.Testing.UnitTesting;

internal static class MemoryLeakHelper
{
    public static void CaptureMiniDump()
    {
        var dumper = BackstageServiceFactory.ServiceProvider.GetBackstageService<IMiniDumper>();
        dumper?.Write( new MiniDumpOptions( false ) );
    }
    
    public static void CaptureDotMemoryDumpAndThrow()
    {
        DotMemory.EnsurePrerequisite();
        var dotMemoryConfig = new DotMemory.Config();
        var path = Path.Combine( Path.GetTempPath(), "Metalama", "MemoryDumps" );
        dotMemoryConfig.SaveToDir( path );

        DotMemory.GetSnapshotOnce( dotMemoryConfig );

        throw new AssertionFailedException( $"A memory leak was detected. Inspect the dump file in '{path}'." );
    }
}

#endif