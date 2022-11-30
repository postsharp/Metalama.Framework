using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.CompileTimeIf
{
    internal class CompileTimeIfAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            if (meta.Target.Method.IsStatic)
            {
                Console.WriteLine($"Invoking {meta.Target.Method.ToDisplayString()}");
            }
            else
            {
                Console.WriteLine($"Invoking {meta.Target.Method.ToDisplayString()} on instance {meta.This.ToString()}.");
            }

            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetCode
    {
        [CompileTimeIf]
        public void InstanceMethod()
        {
            Console.WriteLine("InstanceMethod");
        }

        [CompileTimeIf]
        public static void StaticMethod()
        {
            Console.WriteLine("StaticMethod");
        }
    }
}
