// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Creates a specific instance of the <see cref="BaseTestRunner"/> class.
    /// </summary>
    internal interface ITestRunnerFactory
    {
        BaseTestRunner CreateTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            MetadataReference[] metadataReferences,
            ITestOutputHelper? logger );
    }
}