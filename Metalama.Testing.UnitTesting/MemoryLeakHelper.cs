// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NET6_0_OR_GREATER || NETFRAMEWORK
using JetBrains.Profiler.SelfApi;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine;
using System.IO;
using System.Threading;

namespace Metalama.Testing.UnitTesting;

internal static class MemoryLeakHelper
{
    private static int _miniDumps;

    public static void CaptureMiniDumpOnce()
    {
        if ( Interlocked.Increment( ref _miniDumps ) == 1 )
        {
            var dumper = BackstageServiceFactory.ServiceProvider.GetBackstageService<IMiniDumper>();
            dumper?.Write();
        }
        else
        {
            // Do not capture more than one dump during tests because it can make things very slow
            // and give the impression that tests are stuck.
        }
    }

    public static void CaptureDotMemoryDumpAndThrow()
    {
        DotMemory.Init();
        var dotMemoryConfig = new DotMemory.Config();
        var path = Path.Combine( MetalamaPathUtilities.GetTempPath(), "Metalama", "MemoryDumps" );
        dotMemoryConfig.SaveToDir( path );

        DotMemory.GetSnapshotOnce( dotMemoryConfig );

        throw new AssertionFailedException( $"A memory leak was detected. Inspect the dump file in '{path}'." );
    }
}
#endif