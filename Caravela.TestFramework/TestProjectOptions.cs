// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Options;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Caravela.TestFramework
{
    public class TestProjectOptions : DefaultDirectoryOptions, IProjectOptions, IDisposable
    {
        public TestProjectOptions()
        {
            var directory = Path.Combine( Path.GetTempPath(), "Caravela.TestFramework", Guid.NewGuid().ToString() );
            this.CompileTimeProjectCacheDirectory = directory;

            Directory.CreateDirectory( directory );
        }

        public bool DebugCompilerProcess => false;

        public bool DebugAnalyzerProcess => false;

        public bool DebugIdeProcess => false;

        public override string CompileTimeProjectCacheDirectory { get; }

        public string ProjectId => "test";

        public string? BuildTouchFile => null;

        public string? AssemblyName => null;

        public ImmutableArray<object> PlugIns => ImmutableArray<object>.Empty;

        public void Dispose()
        {
            if ( Directory.Exists( this.CompileTimeProjectCacheDirectory ) )
            {
                Directory.Delete( this.CompileTimeProjectCacheDirectory, true );
            }
        }
    }
}