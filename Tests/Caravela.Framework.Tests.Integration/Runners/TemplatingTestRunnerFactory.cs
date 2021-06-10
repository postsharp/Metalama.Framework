// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class TemplatingTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner( IServiceProvider serviceProvider, string? projectDirectory, MetadataReference[] metadataReferences )
            => new TemplatingTestRunner( serviceProvider, projectDirectory, metadataReferences );
    }
}