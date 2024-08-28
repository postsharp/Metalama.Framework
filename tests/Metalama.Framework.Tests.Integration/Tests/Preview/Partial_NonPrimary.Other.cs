using System;
using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Tests.Preview.Partial_NonPrimary;

internal partial class TargetClass
{
    [TestAspect]
    public void Bar() { }
}