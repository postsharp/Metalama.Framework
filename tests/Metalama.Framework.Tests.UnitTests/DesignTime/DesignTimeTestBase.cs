// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public abstract class DesignTimeTestBase : FrameworkBaseTestClass
{
    protected DesignTimeTestBase( ITestOutputHelper? logger = null ) : base( logger ) { }

    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService<IMetalamaProjectClassifier>( _ => new TestMetalamaProjectClassifier() );
        services.AddGlobalService<UserDiagnosticRegistrationService>( provider => new UserDiagnosticRegistrationService( provider ) );
    }
}