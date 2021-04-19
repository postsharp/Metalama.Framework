// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
{
    internal class Options : IBuildOptions
    {
        public bool AttachDebugger => throw new NotImplementedException();

        public bool MapPdbToTransformedCode => throw new NotImplementedException();

        public string? CompileTimeProjectDirectory => null;

        public string? CrashReportDirectory => null;
    }
}