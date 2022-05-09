// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.TestFramework;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    // ReSharper disable once UnusedType.Global
    internal class DesignTimeTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            => new DesignTimeTestRunner( serviceProvider, projectDirectory, references, logger );
    }
}