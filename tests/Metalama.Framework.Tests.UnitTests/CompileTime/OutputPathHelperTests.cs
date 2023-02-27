// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Maintenance;
using Metalama.Framework.Engine.CompileTime;
using System;
using System.IO;
using System.Runtime.Versioning;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime;

public sealed class OutputPathHelperTests
{
    private readonly TestTempFileManager _tempFileManager = new();

    [Theory]
    [InlineData( "Metalama.Extensions.DependencyInjection.ServiceLocator", ".NetStandard,Version=v2.0" )]
    public void LengthUnder256( string assemblyName, string frameworkName )
    {
        var outputPathHelper = new OutputPathHelper( this._tempFileManager );
        var paths = outputPathHelper.GetOutputPaths( assemblyName, new FrameworkName( frameworkName ), ulong.MaxValue );
        Assert.True( paths.Pe.Length < 256 );
        Assert.True( paths.Pdb.Length < 256 );
        Assert.True( paths.Manifest.Length < 256 );
    }

    private sealed class TestTempFileManager : ITempFileManager
    {
        // We hardcode a long directory so that it does not change according to test environments.
        public string GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, string? subsubirectory, bool versionNeutral )
            => Path.Combine( @"C:\Users\GaelFraiteur.AzureAD\AppData\Local\Temp\Metalama", subdirectory, "0.5.53.1189-local-GaelFraiteur-debug" );
    }
}