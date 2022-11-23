// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Services;
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
            ProjectServiceProvider serviceProvider,
            TestProjectReferences references,
            ITestOutputHelper? logger )
        {
            if ( logger != null && testInput.Options.EnableLogging.GetValueOrDefault() )
            {
                serviceProvider = serviceProvider.Underlying.WithUntypedService( typeof(ILoggerFactory), new XunitLoggerFactory( logger ) );
            }

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
                    factoryType = Type.GetType( testInput.Options.TestRunnerFactoryType!, true )!;
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