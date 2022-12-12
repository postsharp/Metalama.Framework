// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public abstract class DesignTimeTestBase : UnitTestClass
{
    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        services.AddGlobalService<IMetalamaProjectClassifier>( _ => new TestMetalamaProjectClassifier() );
    }
}