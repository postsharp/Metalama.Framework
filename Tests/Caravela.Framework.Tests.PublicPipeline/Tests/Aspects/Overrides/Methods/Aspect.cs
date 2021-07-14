﻿using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.Simple
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("This is the overriding method.");
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        public void TargetMethod_Void(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
        }

        [Override]
        public int TargetMethod_Int()
        {
            Console.WriteLine("This is the original method.");
            return 42;
        }

        [Override]
        public int TargetMethod_Int(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
            return x + y;
        }

        [Override]
        public static void TargetMethod_Static()
        {
            Console.WriteLine("This is the original static method.");
        }

        [Override]
        public void TargetMethod_Out(out int x)
        {
            Console.WriteLine("This is the original method.");
            x = 42;
        }

        [Override]
        public void TargetMethod_Ref(ref int x)
        {
            Console.WriteLine("This is the original method.");
            x = 42;
        }

        [Override]
        public void TargetMethod_In(in int x)
        {
            Console.WriteLine("This is the original method.");
        }
    }
}
