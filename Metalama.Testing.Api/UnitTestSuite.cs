﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Testing.Api.Options;
using Metalama.Testing.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit.Abstractions;

namespace Metalama.Testing.Api
{
    /// <summary>
    /// A base class for all Metalama unit tests that require Metalama services. Exposes a <see cref="CreateTestContext(Metalama.Framework.Engine.Services.AdditionalServiceCollection)"/>
    /// that creates a context with all services. The next step is typically to call one of the methods or properties of the returned <see cref="TestContext"/>.
    /// </summary>
    public abstract class UnitTestSuite
    {
        static UnitTestSuite()
        {
            TestingServices.Initialize();
        }

        private readonly ITestOutputHelper? _testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestSuite"/> class.
        /// </summary>
        /// <param name="testOutputHelper"></param>
        protected UnitTestSuite( ITestOutputHelper? testOutputHelper = null )
        {
            this._testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Gets an object allowing to write to the test output. 
        /// </summary>
        protected ITestOutputHelper TestOutput => this._testOutputHelper.AssertNotNull();

        private void AddXunitLogging( AdditionalServiceCollection testServices )
        {
            // If we have an Xunit test output, override the logger.
            if ( this._testOutputHelper != null )
            {
                var loggerFactory = new XunitLoggerFactory( this._testOutputHelper );
                testServices.BackstageServices.Add( _ => loggerFactory );
            }
        }

        /// <summary>
        /// Adds services or mocks that are common to all tests in the current class. This method is called
        /// by <see cref="CreateTestContext(Metalama.Framework.Engine.Services.AdditionalServiceCollection)"/> and the
        /// <paramref name="services"/> parameter is the one passed to the <see cref="CreateTestContext(Metalama.Framework.Engine.Services.AdditionalServiceCollection)"/>, if any,
        /// or an empty collection otherwise.
        /// </summary>
        protected virtual void ConfigureServices( AdditionalServiceCollection services )
        {
            this.AddXunitLogging( services );
        }

        /// <summary>
        /// Creates a test context with a collection of additional services or mocks.
        /// </summary>
        protected TestContext CreateTestContext( AdditionalServiceCollection service ) => this.CreateTestContext( null, service );

        /// <summary>
        /// Creates a test context, optionally with a non-default <see cref="TestProjectOptions"/> or a collection of additional services or mocks.
        /// </summary>
        protected TestContext CreateTestContext( TestProjectOptions? projectOptions = null, AdditionalServiceCollection? mockFactory = null )
            => new(
                projectOptions ?? new TestProjectOptions( additionalAssemblies: ImmutableArray.Create( this.GetType().Assembly ) ),
                null,
                this.GetMockServices( mockFactory ) );

        private AdditionalServiceCollection GetMockServices( AdditionalServiceCollection? arg )
        {
            var services = arg ?? new AdditionalServiceCollection();
            this.ConfigureServices( services );

            return services;
        }
    }
}