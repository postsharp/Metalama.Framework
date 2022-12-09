// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestClass"/> class.
        /// </summary>
        /// <param name="testOutputHelper"></param>
        protected UnitTestClass( ITestOutputHelper? testOutputHelper = null )
        {
            this._testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Gets an object allowing to write to the test output. 
        /// </summary>
        protected ITestOutputHelper TestOutput => this._testOutputHelper.AssertNotNull();

        private void AddXunitLogging( IAdditionalServiceCollection testServices )
        {
            // If we have an Xunit test output, override the logger.
            if ( this._testOutputHelper != null )
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
        protected static IAdditionalServiceCollection CreateAdditionalServiceCollection( params IService[] services )
        {
            return new AdditionalServiceCollection( services );
        }

        protected virtual TestContext CreateTestContext() => this.CreateTestContext( null, null );

        /// <summary>
        /// Creates a test context with a collection of additional services or mocks.
        /// </summary>
        protected virtual TestContext CreateTestContext( IAdditionalServiceCollection service ) => this.CreateTestContext( null, service );

        /// <summary>
        /// Creates a test context, optionally with a non-default <see cref="TestProjectOptions"/> or a collection of additional services or mocks.
        /// </summary>
        protected virtual TestContext CreateTestContext( TestContextOptions? projectOptions, IAdditionalServiceCollection? services = null )
            => new(
                projectOptions ?? new TestContextOptions { AdditionalAssemblies = ImmutableArray.Create( this.GetType().Assembly ) },
                this.GetMockServices( services ) );

        private IAdditionalServiceCollection GetMockServices( IAdditionalServiceCollection? arg )
        {
            var services = arg ?? new AdditionalServiceCollection();
            this.ConfigureServices( services );

            return services;
        }
    }
}