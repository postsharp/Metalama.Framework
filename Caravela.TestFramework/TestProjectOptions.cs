// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Options;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Caravela.TestFramework
{
    /// <summary>
    /// An implementation of <see cref="IProjectOptions"/> and <see cref="IPathOptions"/> that can be used in tests.
    /// </summary>
    public class TestProjectOptions : DefaultPathOptions, IProjectOptions, IDisposable
    {
        public TestProjectOptions()
        {
            var directory = Path.Combine( Path.GetTempPath(), "Caravela", "Tests", Guid.NewGuid().ToString() );
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

        public bool IsFrameworkEnabled => true;

        public bool FormatOutput => false;

        public bool FormatCompileTimeCode { get; set; }

        public bool IsUserCodeTrusted => true;

        public IProjectOptions Apply( IProjectOptions options ) => options;

        public void Dispose()
        {
            if ( Directory.Exists( this.CompileTimeProjectCacheDirectory ) )
            {
                Directory.Delete( this.CompileTimeProjectCacheDirectory, true );
            }
        }
    }
}