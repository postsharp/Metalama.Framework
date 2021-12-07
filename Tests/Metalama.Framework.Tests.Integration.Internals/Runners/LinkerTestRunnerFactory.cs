// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Pipeline;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal class LinkerTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            MetadataReference[] metadataReferences,
            ITestOutputHelper? logger )
            => new LinkerTestRunner( serviceProvider, projectDirectory, metadataReferences, logger );
    }
}