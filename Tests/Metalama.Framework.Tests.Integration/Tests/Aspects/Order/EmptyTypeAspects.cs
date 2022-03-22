// @Skipped(#30089)

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Order.EmptyTypeAspects;

[assembly: AspectOrder( typeof(Aspect3), typeof(Aspect2), typeof(Aspect1) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Order.EmptyTypeAspects
{
    public class AspectBase : TypeAspect
    {
    }

    public class Aspect1 : AspectBase
    {
    }

    public class Aspect2 : AspectBase
    {
    }

    public class Aspect3 : AspectBase
    {
    }

    // <target>
    [Aspect1]
    [Aspect2]
    [Aspect3]
    internal class TargetClass
    {
        public int Method()
        {
            return 42;
        }
    }
}