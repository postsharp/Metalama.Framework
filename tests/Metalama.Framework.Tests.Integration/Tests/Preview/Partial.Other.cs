﻿using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Preview.Partial;

internal partial class TargetClass
{
    [TestAspect]
    public void Bar() { }
}