﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_FinalInvoker
{
    public class TestAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            return meta.Target.Method.Invokers.Final!.Invoke(meta.This, meta.Target.Method.Parameters[0].Value);
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
