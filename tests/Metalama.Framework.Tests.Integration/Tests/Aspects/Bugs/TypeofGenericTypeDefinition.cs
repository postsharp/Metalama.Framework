using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TypeofGenericTypeDefinition;

internal class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return TypeFactory.GetType(typeof(List<>)).ToTypeOfExpression().Value;
    }
}

internal class TargetCode
{
    // <target>
    [TestAspect]
    object? Method() => null;
}