// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Testing.AspectTesting;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    // ReSharper disable once UnusedType.Global
    internal class DesignTimeTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            GlobalServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            => new DesignTimeTestRunner( serviceProvider, projectDirectory, references, logger );
    }
}