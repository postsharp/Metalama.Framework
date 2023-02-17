﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System.Linq;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Properties.RefProperty;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var m = meta.Target.Type.Methods.OfName("M").Single();

        INamedType mappingType = (INamedType)meta.Target.Method.Parameters[0].Type;

        var from = meta.Target.Method.Parameters[0];

        foreach (var fieldOrProperty in mappingType.FieldsAndProperties)
        {
            m.Invoke(fieldOrProperty);
        }

        return meta.Proceed();
    }
}

// <target>
internal class TargetClass
{
    void M(ref int i) { }

    public int F;

    public ref int P => ref F;

    [Test]
    public void Map(TargetClass source) { }
}