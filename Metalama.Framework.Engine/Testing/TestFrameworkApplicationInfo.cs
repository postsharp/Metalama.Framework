// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Framework.Engine.Testing;

public class TestFrameworkApplicationInfo : ApplicationInfoBase
{
    public TestFrameworkApplicationInfo() : base( typeof(TestFrameworkApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.TestFramework";

    public override bool ShouldCreateLocalCrashReports => false;
}