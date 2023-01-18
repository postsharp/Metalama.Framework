﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DistributedDesignTimeTestBase : UnitTestClass
{
    protected DistributedDesignTimeTestBase( ITestOutputHelper? logger = null ) : base( logger ) { }

    protected override TestContext CreateTestContextCore( TestContextOptions projectOptions, IAdditionalServiceCollection services )
    {
        return new DistributedDesignTimeTestContext( projectOptions, services );
    }

    private protected DistributedDesignTimeTestContext CreateDistributedDesignTimeTestContext(
        ServiceProviderBuilder<IGlobalService>? userProcessServices,
        ServiceProviderBuilder<IGlobalService>? analysisProcessServices,
        TestContextOptions? options )
    {
        var services = new AdditionalServiceCollection();
        services.AddGlobalService( provider => new TestWorkspaceProvider( provider ) );
        var context = (DistributedDesignTimeTestContext) this.CreateTestContext( options, services );
        _ = context.InitializeAsync( userProcessServices, analysisProcessServices );

        return context;
    }
}