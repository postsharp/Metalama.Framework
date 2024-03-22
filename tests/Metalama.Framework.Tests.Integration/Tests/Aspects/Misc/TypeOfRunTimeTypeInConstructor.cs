using Metalama.Framework.Aspects;

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