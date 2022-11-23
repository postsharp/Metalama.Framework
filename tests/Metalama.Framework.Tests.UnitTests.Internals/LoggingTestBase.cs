// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Project;
using Metalama.TestFramework;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests
{
    public class LoggingTestBase : TestBase
    {
        private readonly ITestOutputHelper? _testOutputHelper;

        protected LoggingTestBase( ITestOutputHelper? testOutputHelper = null )
        {
            this._testOutputHelper = testOutputHelper;
        }

        protected ITestOutputHelper Logger => this._testOutputHelper.AssertNotNull();

        protected override void ConfigureDefaultServices( TestServiceFactory services )
        {
            this.AddXunitLogging( services );
        }

        protected void AddXunitLogging( TestServiceFactory services )
        {
            // If we have an Xunit test output, override the logger.
            if ( this._testOutputHelper != null )
            {
                var loggerFactory = new XunitLoggerFactory( this._testOutputHelper );
                services.BackstageServices.Add( _ => loggerFactory );
            }
        }
    }
}