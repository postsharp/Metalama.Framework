﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.Accessibility
{
    /*
     * Tests that overriding methods preserves accessibility.
     */

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
        private void PrivateMethod()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        private protected void PrivateProtectedMethod()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        protected void ProtectedMethod()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        internal void InternalMethod()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        protected internal void ProtectedInternalMethod()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        public void PublicMethod()
        {
            Console.WriteLine("This is the original method.");
        }
    }
}
