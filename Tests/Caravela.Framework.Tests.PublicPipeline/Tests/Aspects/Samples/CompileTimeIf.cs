using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.CompileTimeIf
{
    internal class CompileTimeIfAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            if (meta.Method.IsStatic)
            {
                Console.WriteLine($"Invoking {meta.Method.ToDisplayString()}");
            }
            else
            {
                Console.WriteLine($"Invoking {meta.Method.ToDisplayString()} on instance {meta.This.ToString()}.");
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
