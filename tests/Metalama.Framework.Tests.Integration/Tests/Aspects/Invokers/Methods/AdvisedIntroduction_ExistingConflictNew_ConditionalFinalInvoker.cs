using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictNew_ConditionalFinalInvoker;

[assembly: AspectOrder(typeof(TestIntroductionAttribute), typeof(TestAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictNew_ConditionalFinalInvoker
{
    public class TestIntroductionAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.New)]
        public void VoidMethod()
        {
            meta.InsertComment("Introduced.");
            System.Console.WriteLine("Introduced method print.");
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int? ExistingMethod()
        {
            meta.InsertComment("Introduced.");
            return 100;
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int? ExistingMethod_Parameterized(int x)
        {
            meta.InsertComment("Introduced.");
            return x + 100;
        }
    }

    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advice.Override(method, nameof(MethodTemplate));
            }
        }

        [Template]
        public dynamic MethodTemplate()
        {
            if (meta.Target.Method.Parameters.Count == 0)
            {
                return meta.Target.Method.Invokers.ConditionalFinal!.Invoke(meta.This);
            }
            else
            {
                return meta.Target.Method.Invokers.ConditionalFinal!.Invoke(meta.This, meta.Target.Method.Parameters[0].Value);
            }
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass
    {
        public void VoidMethod()
        {
            System.Console.WriteLine("Base method print.");
        }

        public int? ExistingMethod()
        {
            return 42;
        }

        public int? ExistingMethod_Parameterized(int x)
        {
            return x;
        }
    }
}
