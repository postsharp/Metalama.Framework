using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_BaseInvoker
{
    public class TestAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            return meta.Target.Method.Invokers.Base!.Invoke(meta.This, meta.Target.Method.Parameters[0].Value);
        }
    }

    // <target>
    internal class TargetClass
    {
        [Test]
        public int Method(int x)
        {
            return x;
        }
    }
}
