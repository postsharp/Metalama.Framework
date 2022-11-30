// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Testing;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public abstract class DesignTimeTestBase : TestBase
{
    protected override void ConfigureServices( AdditionalServiceCollection testServices )
    {
        testServices.GlobalServices.Add( _ => new TestMetalamaProjectClassifier() );
    }
}