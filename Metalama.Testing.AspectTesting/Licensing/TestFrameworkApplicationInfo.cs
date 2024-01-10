// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;

namespace Metalama.Testing.AspectTesting.Licensing;

internal sealed class TestFrameworkApplicationInfo : ApplicationInfoBase
{
    public TestFrameworkApplicationInfo() : base( typeof(TestFrameworkApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.Testing.AspectTesting";

    public override bool ShouldCreateLocalCrashReports => false;
}