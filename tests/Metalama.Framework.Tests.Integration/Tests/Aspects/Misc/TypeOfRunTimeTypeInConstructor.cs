using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.TypeOfRunTimeTypeInConstructor;

class TestAspect : MethodAspect
{
    public TestAspect()
    {
        _ = typeof(RunTimeClass);
    }
}

class RunTimeClass { }

// <target>
public partial class TargetClass
{
    [TestAspect]
    void M() { }
}