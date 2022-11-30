// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Testing.Api;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public abstract class DesignTimeTestBase : UnitTestSuite
{
    protected override void ConfigureServices(IAdditionalServiceCollection services )
    {
        services.AddService( _ => new TestMetalamaProjectClassifier() );
    }
}