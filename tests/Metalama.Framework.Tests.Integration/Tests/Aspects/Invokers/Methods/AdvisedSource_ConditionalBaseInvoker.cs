using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_ConditionalBaseInvoker
{
    public class TestAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            TargetClass? local = null;

            if (meta.Target.Method.Parameters.Count == 0)
            {
                return meta.Target.Method.With( local, InvokerOptions.Base | InvokerOptions.NullConditional ).Invoke();
            }
            else
            {
                return meta.Target.Method.With( local, InvokerOptions.Base | InvokerOptions.NullConditional).Invoke( meta.Target.Method.Parameters[0].Value );
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Test]
        public void VoidMethod() { }

        [Test]
        public int? Method( int? x )
        {
            return x;
        }
    }
}