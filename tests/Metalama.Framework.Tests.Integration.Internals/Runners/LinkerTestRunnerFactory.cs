﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Testing.AspectTesting;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal class LinkerTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            ProjectServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            => new LinkerTestRunner( serviceProvider, projectDirectory, references, logger );
    }
}