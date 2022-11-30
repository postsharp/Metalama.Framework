﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Testing.Api;

internal class TestApiApplicationInfo : ApplicationInfoBase
{
    public TestApiApplicationInfo() : base( typeof(TestApiApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.Testing.Api";

    public override bool ShouldCreateLocalCrashReports => false;
}