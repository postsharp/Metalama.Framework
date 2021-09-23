using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherType_FinalInvoker
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute, IAspect<IMethod>
    {
        public void BuildAspect(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.Advices.OverrideMethod(aspectBuilder.Target, nameof(OverrideMethod));
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            var parameterType = (INamedType)( (IExpression)meta.Target.Method.Parameters[0] ).Type;
            var barMethod = parameterType.Methods.OfName("Bar").First();
            return barMethod.Invokers.Final!.Invoke(meta.Target.Method.Parameters[0].Value);
        }
    }

    internal class OtherClass
    {
        public void Bar()
        {
        }
    }       

    // <target>
    internal class TargetClass
    {
        [TestAttribute]
        public void Foo(OtherClass other)
        {
        }
    }
}
