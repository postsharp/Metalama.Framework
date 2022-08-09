// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Framework.Engine.Testing;

internal class TestFrameworkApplicationInfo : ApplicationInfoBase
{
    public TestFrameworkApplicationInfo() : base( typeof(TestFrameworkApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.TestFramework";

    public override bool ShouldCreateLocalCrashReports => false;
}