// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class DesignTimeTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner( IServiceProvider serviceProvider, string? projectDirectory, IEnumerable<Assembly>? requiredAssemblies )
            => new DesignTimeTestRunner( serviceProvider, projectDirectory );
    }
}