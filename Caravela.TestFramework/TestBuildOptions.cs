// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;

namespace Caravela.TestFramework
{
    public class TestBuildOptions : IBuildOptions
    {
        public virtual bool AttachDebugger => false;

        public virtual bool MapPdbToTransformedCode => false;

        public virtual string? CompileTimeProjectDirectory => null;

        public virtual string? CrashReportDirectory => null;
    }
}