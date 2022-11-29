﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Testing;
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

        protected override void ConfigureServices( AdditionalServiceCollection testServices )
        {
            this.AddXunitLogging( testServices );
        }

        protected void AddXunitLogging( AdditionalServiceCollection testServices )
        {
            // If we have an Xunit test output, override the logger.
            if ( this._testOutputHelper != null )
            {
                var loggerFactory = new XunitLoggerFactory( this._testOutputHelper );
                testServices.BackstageServices.Add( _ => loggerFactory );
            }
        }
    }
}