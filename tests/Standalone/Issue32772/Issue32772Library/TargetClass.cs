using Metalama.Framework.Aspects;

#nullable enable

namespace Issue32772Library
{
    public class TestAspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            return meta.Proceed();
        }
    }

    public class TargetClass
    {
        [TestAspect]
        public void Foo()
        {
        }
    }
}