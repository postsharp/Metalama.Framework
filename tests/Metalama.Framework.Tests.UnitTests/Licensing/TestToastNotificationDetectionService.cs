﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.UserInterface;

namespace Metalama.Framework.Tests.UnitTests.Licensing;

public class TestToastNotificationDetectionService : IToastNotificationDetectionService
{
    public bool WasDetectionTriggered { get; private set; }

    public void Detect() => this.WasDetectionTriggered = true;
}