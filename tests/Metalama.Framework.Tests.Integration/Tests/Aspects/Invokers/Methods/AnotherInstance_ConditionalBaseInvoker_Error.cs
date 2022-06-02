#if TEST_OPTIONS
// @Skipped(#28907 Linker: conditional access expression)
# endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherInstance_ConditionalBaseInvoker_Error
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            var voidMethodOverrideBuilder = aspectBuilder.Advice.IntroduceMethod(aspectBuilder.Target, nameof(OverrideMethod), whenExists: OverrideStrategy.Override);
            voidMethodOverrideBuilder.Name = "VoidMethod";
            voidMethodOverrideBuilder.ReturnType = TypeFactory.GetType(SpecialType.Void);

            var methodOverrideBuilder = aspectBuilder.Advice.IntroduceMethod(aspectBuilder.Target, nameof(OverrideMethod), whenExists: OverrideStrategy.Override);
            methodOverrideBuilder.Name = "Method";
            methodOverrideBuilder.ReturnType = TypeFactory.GetType(SpecialType.Int32);
            methodOverrideBuilder.AddParameter("x", TypeFactory.GetType(SpecialType.Int32));
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            var x = meta.This;

            if (meta.Target.Method.Parameters.Count == 0)
            {
                return meta.Target.Method.Invokers.ConditionalBase!.Invoke(x);
            }
            else
            {
                return meta.Target.Method.Invokers.ConditionalBase!.Invoke(x, meta.Target.Method.Parameters[0].Value);
            }
        }
    }

    internal class BaseClass
    {
        public virtual void VoidMethod() { }

        public virtual int Method(int x)
        {
            return x;
        }
    }

    // <target>
    [TestAttribute]
    internal class TargetClass : BaseClass { }
}