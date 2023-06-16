using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.TypeOfRunTimeTypeInConstructor;

class TestAspect : TypeAspect
{
    public TestAspect()
    {
        _ = typeof(RunTimeClass);
    }
}

class RunTimeClass { }

// <target>
[TestAspect]
class TargetClass { }