// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Testing.UnitTesting;
using System;
using Xunit.Abstractions;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// Instantiates a specific implementation of the  <see cref="BaseTestRunner"/> class.
    /// </summary>
    internal static class TestRunnerFactory
    {
        public static BaseTestRunner CreateTestRunner(
            TestInput testInput,
            GlobalServiceProvider serviceProvider,
            TestProjectReferences references,
            ITestOutputHelper? logger )
        {
            if ( logger != null && testInput.Options.EnableLogging.GetValueOrDefault() )
            {
                serviceProvider = serviceProvider.Underlying.WithUntypedService( typeof(ILoggerFactory), new XunitLoggerFactory( logger ) );
            }

            // Create the ILicenseKeyProvider.
            ILicenseKeyProvider? licenseKeyProvider;

            if ( !string.IsNullOrEmpty( testInput.Options.LicenseKeyProviderType ) )
            {
                var typeName = testInput.Options.LicenseKeyProviderType;

                if ( !typeName.ContainsOrdinal( ',' ) && testInput.ProjectProperties.AssemblyName != null )
                {
                    typeName = $"{typeName}, {testInput.ProjectProperties.AssemblyName}";
                }

                var licenseKeyProviderType = Type.GetType( typeName ).AssertNotNull();
                licenseKeyProvider = (ILicenseKeyProvider) Activator.CreateInstance( licenseKeyProviderType )!;
            }
            else
            {
                licenseKeyProvider = null;
            }

            // Create the ITestRunnerFactory.
            ITestRunnerFactory testRunnerFactory;

            if ( !string.IsNullOrEmpty( testInput.Options.TestRunnerFactoryType ) )
            {
                Type? factoryType;

                try
                {
                    var typeName = testInput.Options.TestRunnerFactoryType!;

                    if ( !typeName.ContainsOrdinal( ',' ) && testInput.ProjectProperties.AssemblyName != null )
                    {
                        typeName = $"{typeName}, {testInput.ProjectProperties.AssemblyName}";
                    }

                    factoryType = Type.GetType( typeName, true )!;
                }
                catch ( Exception e )
                {
                    throw new InvalidOperationException( $"Cannot instantiate the type '{testInput.Options.TestRunnerFactoryType}': {e.Message}" );
                }

                testRunnerFactory = (ITestRunnerFactory) Activator.CreateInstance( factoryType )!;
            }
            else
            {
                switch ( testInput.Options.TestScenario )
                {
                    case TestScenario.DesignTime:
                        testRunnerFactory = new DesignTimeTestRunnerFactory();

                        break;

                    case TestScenario.Preview:
                        testRunnerFactory = new PreviewTestRunnerFactory();

                        break;

                    case TestScenario.PreviewLiveTemplate:
                    case TestScenario.ApplyLiveTemplate:
                        testRunnerFactory = new LiveTemplateTestRunnerFactory();

                        break;

                    default:
                        testRunnerFactory = new AspectTestRunnerFactory();

                        break;
                }
            }

            return testRunnerFactory.CreateTestRunner(
                serviceProvider,
                testInput.ProjectDirectory,
                references,
                logger,
                licenseKeyProvider );
        }
    }
}