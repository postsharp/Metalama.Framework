using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Methods.NameConflict_Introduced;
using System.Linq;

[assembly: AspectOrder(typeof(InnerOverrideAttribute), typeof(OuterOverrideAttribute), typeof(IntroductionAttribute))]
#pragma warning disable CS0219

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Methods.NameConflict_Introduced
{
    public class InnerOverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach(var method in builder.Target.Methods.Where(m => !m.IsImplicitlyDeclared))
            {
                builder.Advise.Override(method, nameof(OverrideMethod));
            }
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            int i = 27;
            return meta.Proceed();
        }
    }

    public class OuterOverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var method in builder.Target.Methods.Where(m => !m.IsImplicitlyDeclared))
            {
                builder.Advise.Override(method, nameof(OverrideMethod));
            }
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            int i = 42;
            return meta.Proceed();
        }
    }

    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public dynamic? IntroducedMethod_ConflictBetweenOverrides()
        {
            return meta.Proceed();
        }

        [Introduce]
        public dynamic? IntroducedMethod_ConflictWithParameter( int i)
        {
            return meta.Proceed();
        }
    }

    // <target>
    [InnerOverride]
    [OuterOverride]
    [Introduction]
    internal class TargetClass
    {
    }
}
