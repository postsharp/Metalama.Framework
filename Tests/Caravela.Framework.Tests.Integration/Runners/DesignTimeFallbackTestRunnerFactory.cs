// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    // ReSharper disable once UnusedType.Global
    internal class DesignTimeFallbackTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            MetadataReference[] metadataReferences,
            ITestOutputHelper? logger )
            => new DesignTimeFallbackTestRunner( serviceProvider, projectDirectory, metadataReferences, logger );
    }
}