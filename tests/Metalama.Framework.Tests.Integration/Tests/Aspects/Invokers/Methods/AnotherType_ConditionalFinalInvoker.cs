using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherType_ConditionalFinalInvoker
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
            var otherClassMethod = parameterType.Methods.OfName( meta.Target.Method.Name ).First();

            if (otherClassMethod.Parameters.Count == 0)
            {
                return otherClassMethod.With(meta.Target.Method.Parameters[0], InvokerOptions.NullConditional ).Invoke();
            }
            else
            {
                return otherClassMethod.With(meta.Target.Method.Parameters[0], InvokerOptions.NullConditional )
                    .Invoke( meta.Target.Method.Parameters[1].Value );
            }
        }
    }

    internal class OtherClass
    {
        public void VoidMethod() { }

        public int? Method( int? x )
        {
            return x;
        }
    }

    // <target>
    internal class TargetClass
    {
        [TestAttribute]
        public void VoidMethod( OtherClass other ) { }

        [TestAttribute]
        public int? Method( OtherClass other, int? x )
        {
            return x;
        }
    }
}