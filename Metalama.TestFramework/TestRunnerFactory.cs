// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;
using System;
using Xunit.Abstractions;

namespace Metalama.TestFramework
{
    /// <summary>
    /// Instantiates a specific implementation of the  <see cref="BaseTestRunner"/> class.
    /// </summary>
    internal static class TestRunnerFactory
    {
        public static BaseTestRunner CreateTestRunner(
            TestInput testInput,
            ServiceProvider serviceProvider,
            TestProjectReferences references,
            ITestOutputHelper? logger )
        {
            if ( string.IsNullOrEmpty( testInput.Options.TestRunnerFactoryType ) )
            {
                return new AspectTestRunner(
                    serviceProvider,
                    testInput.ProjectDirectory,
                    references,
                    logger );
            }
            else
            {
                Type? factoryType;

                try
                {
                    factoryType = Type.GetType( testInput.Options.TestRunnerFactoryType, true )!;
                }
                catch ( Exception e )
                {
                    throw new InvalidOperationException( $"Cannot instantiate the type '{testInput.Options.TestRunnerFactoryType}': {e.Message}" );
                }

                var testRunnerFactory = (ITestRunnerFactory) Activator.CreateInstance( factoryType )!;

                return testRunnerFactory.CreateTestRunner(
                    serviceProvider,
                    testInput.ProjectDirectory,
                    references,
                    logger );
            }
        }
    }
}