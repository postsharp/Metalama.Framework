// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Xunit.Abstractions;

namespace Metalama.TestFramework
{
    /// <summary>
    /// Creates a specific instance of the <see cref="BaseTestRunner"/> class.
    /// </summary>
    public interface ITestRunnerFactory
    {
        BaseTestRunner CreateTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger );
    }
}