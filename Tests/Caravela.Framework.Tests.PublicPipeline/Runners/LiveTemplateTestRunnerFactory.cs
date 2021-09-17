// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    public class LiveTemplateTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            MetadataReference[] metadataReferences,
            ITestOutputHelper? logger )
            => new LiveTemplateTestRunner( serviceProvider, projectDirectory, metadataReferences, logger );
    }
}