// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Testing;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public abstract class DesignTimeTestBase : TestBase
{
    protected override ServiceProvider ConfigureServiceProvider( ServiceProvider serviceProvider )
        => base.ConfigureServiceProvider( serviceProvider ).WithService( new TestMetalamaProjectClassifier() );
}