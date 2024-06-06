﻿using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.IntroducedNestedType;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var type = builder.Advice.IntroduceClass(builder.Target, "TestType");
        type.ImplementInterface(typeof(ITestInterface));
    }
}

public interface ITestInterface { }

// <target>
[IntroductionAttribute]
public class TargetType { }