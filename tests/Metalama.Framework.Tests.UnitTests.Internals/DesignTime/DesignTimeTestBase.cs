// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Project;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public abstract class DesignTimeTestBase : TestBase
{
    protected override void ConfigureDefaultServices( TestServiceFactory services ) => new TestServiceFactory( new TestMetalamaProjectClassifier() );
}