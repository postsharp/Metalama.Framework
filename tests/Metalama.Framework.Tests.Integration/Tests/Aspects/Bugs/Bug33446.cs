using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33446;

// <target>
class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return GetNumbers();

        IEnumerable<int> GetNumbers()
        {
            yield return 42;
        }
    }
}