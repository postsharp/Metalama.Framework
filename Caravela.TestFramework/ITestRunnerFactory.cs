// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Creates a specific instance of the <see cref="BaseTestRunner"/> class.
    /// </summary>
    internal interface ITestRunnerFactory
    {
        BaseTestRunner CreateTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            MetadataReference[] metadataReferences,
            ITestOutputHelper? logger );
    }
}