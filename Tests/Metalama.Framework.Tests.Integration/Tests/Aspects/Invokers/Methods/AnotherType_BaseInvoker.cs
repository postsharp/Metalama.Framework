using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherType_BaseInvoker
{
    [AttributeUsage( AttributeTargets.Method )]
    public class TestAttribute : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> aspectBuilder )
        {
            aspectBuilder.Advice.Override( aspectBuilder.Target, nameof(OverrideMethod) );
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            var parameterType = (INamedType)( (IExpression)meta.Target.Method.Parameters[0] ).Type;
            var barMethod = parameterType.Methods.OfName( "Bar" ).First();

            return barMethod.Invokers.Base!.Invoke( meta.Target.Method.Parameters[0].Value );
        }
    }

    internal class OtherClass
    {
        public void Bar() { }
    }

    // <target>
    internal class TargetClass
    {
        [TestAttribute]
        public void Foo( OtherClass other ) { }
    }
}