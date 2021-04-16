// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
{

    internal class Options : IBuildOptions
    {
        public bool AttachDebugger => throw new System.NotImplementedException();

        public bool MapPdbToTransformedCode => throw new System.NotImplementedException();

        public string? CompileTimeProjectDirectory => null;

        public string? CrashReportDirectory => null;
    }
}
