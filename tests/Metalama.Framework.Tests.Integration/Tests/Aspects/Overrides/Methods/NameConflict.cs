using Metalama.Framework.Aspects;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Methods.NameConflict;
using Metalama.Testing.Framework;

[assembly: AspectOrder(typeof(InnerOverrideAttribute), typeof(OuterOverrideAttribute))]
#pragma warning disable CS0219

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Methods.NameConflict
{
    public class InnerOverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            int i = 27;
            return meta.Proceed();
        }
    }

    public class OuterOverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            int i = 42;
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_ConflictBetweenOverrides()
        {
            return 42;
        }

        [InnerOverride]
        public int TargetMethod_ConflictWithTarget()
        {
            int i = 0;
            return 42;
        }

        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_MultipleConflicts()
        {
            int i = 0;
            return 42;
        }
    }
}
