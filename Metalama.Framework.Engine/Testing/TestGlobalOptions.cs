// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Options;
using System;

namespace Metalama.Framework.Engine.Testing;

public class TestGlobalOptions : DefaultGlobalOptions
{
    public override TimeSpan QuietPeriodTimerDelay => TimeSpan.FromMilliseconds( 100 );
}