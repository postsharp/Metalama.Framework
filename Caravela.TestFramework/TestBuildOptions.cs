// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System.Collections.Immutable;

namespace Caravela.TestFramework
{
    public class TestBuildOptions : IBuildOptions
    {
        public bool CompileTimeAttachDebugger => false;

        public bool DesignTimeAttachDebugger => false;

        public virtual bool MapPdbToTransformedCode => false;

        public virtual string? CompileTimeProjectDirectory => null;

        public virtual string? CrashReportDirectory => null;

        public string ProjectId => "test";
        
        public ImmutableArray<object> PlugIns => ImmutableArray<object>.Empty;
    }
}