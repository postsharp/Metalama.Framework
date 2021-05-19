// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Caravela.TestFramework
{
    public class TestBuildOptions : IBuildOptions, IDisposable
    {
        public TestBuildOptions()
        {
            this.CacheDirectory = Path.Combine( Path.GetTempPath(), "Caravela", Guid.NewGuid().ToString() );
            Directory.CreateDirectory( this.CacheDirectory );
        }

        public bool CompileTimeAttachDebugger => false;

        public bool DesignTimeAttachDebugger => false;

        public virtual bool MapPdbToTransformedCode => false;

        public virtual string? CompileTimeProjectDirectory => Environment.CurrentDirectory;

        public virtual string? CrashReportDirectory => null;

        public string CacheDirectory { get; }

        public string ProjectId => "test";

        public string? BuildTouchFile => null;

        public string? AssemblyName => null;

        public ImmutableArray<object> PlugIns => ImmutableArray<object>.Empty;

        public void Dispose()
        {
            Directory.Delete( this.CacheDirectory, true );
        }
    }
}