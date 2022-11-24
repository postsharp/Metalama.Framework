// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.TestFramework;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    public class LiveTemplateTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            ProjectServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            => new LiveTemplateTestRunner( serviceProvider, projectDirectory, references, logger );
    }
}