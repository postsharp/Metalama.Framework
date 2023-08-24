// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using System.Collections.Immutable;
using Xunit.Abstractions;

namespace Metalama.Testing.UnitTesting
{
    /// <summary>
    /// A base class for all Metalama unit tests that require Metalama services. Exposes a <see cref="CreateTestContext(Metalama.Framework.Engine.Services.IAdditionalServiceCollection)"/>
    /// that creates a context with all services. The next step is typically to call one of the methods or properties of the returned <see cref="TestContext"/>.
    /// </summary>
    public abstract class UnitTestClass
    {
        static UnitTestClass()
        {
            TestingServices.Initialize();
        }

        private readonly ITestOutputHelper? _testOutputHelper;
        private readonly bool _injectLoggingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestClass"/> class.
        /// </summary>
        /// <param name="testOutputHelper"></param>
        protected UnitTestClass( ITestOutputHelper? testOutputHelper = null, bool injectLoggingService = true )
        {
            this._testOutputHelper = testOutputHelper;
            this._injectLoggingService = injectLoggingService;
        }

        /// <summary>
        /// Gets an object allowing to write to the test output. 
        /// </summary>
        protected ITestOutputHelper TestOutput => this._testOutputHelper.AssertNotNull();

        private void AddXunitLogging( IAdditionalServiceCollection testServices )
        {
            // If we have an Xunit test output, override the logger.
            if ( this._testOutputHelper != null && this._injectLoggingService )
            {
                var loggerFactory = new XunitLoggerFactory( this._testOutputHelper );
                ((AdditionalServiceCollection) testServices).BackstageServices.Add( _ => loggerFactory );
            }
        }

        /// <summary>
        /// Adds services or mocks that are common to all tests in the current class. This method is called
        /// by <see cref="CreateTestContext(Metalama.Framework.Engine.Services.IAdditionalServiceCollection)"/> and the
        /// <paramref name="services"/> parameter is the one passed to the <see cref="CreateTestContext(Metalama.Framework.Engine.Services.IAdditionalServiceCollection)"/>, if any,
        /// or an empty collection otherwise.
        /// </summary>
        protected virtual void ConfigureServices( IAdditionalServiceCollection services )
        {
            this.AddXunitLogging( services );
        }

        /// <summary>
        /// Creates a collection of additional services that can then be passed to <see cref="CreateTestContext(IAdditionalServiceCollection)"/>.
        /// </summary>
        [PublicAPI]
        protected static IAdditionalServiceCollection CreateAdditionalServiceCollection( params IService[] services )
        {
            return new AdditionalServiceCollection( services );
        }

        protected TestContext CreateTestContext() => this.CreateTestContext( null, null );

        /// <summary>
        /// Creates a test context with a collection of additional services or mocks.
        /// </summary>
        protected TestContext CreateTestContext( IAdditionalServiceCollection service ) => this.CreateTestContext( null, service );

        /// <summary>
        /// Creates a test context, optionally with a non-default <see cref="TestContextOptions"/> or a collection of additional services or mocks.
        /// </summary>
        protected TestContext CreateTestContext( TestContextOptions? contextOptions, IAdditionalServiceCollection? services = null )
            => this.CreateTestContextCore(
                contextOptions ?? new TestContextOptions { AdditionalAssemblies = ImmutableArray.Create( this.GetType().Assembly ) },
                this.GetMockServices( services ) );

        protected virtual TestContext CreateTestContextCore( TestContextOptions contextOptions, IAdditionalServiceCollection services )
            => new( contextOptions, services );

        private IAdditionalServiceCollection GetMockServices( IAdditionalServiceCollection? arg )
        {
            var services = arg ?? new AdditionalServiceCollection();
            this.ConfigureServices( services );

            return services;
        }
    }
}