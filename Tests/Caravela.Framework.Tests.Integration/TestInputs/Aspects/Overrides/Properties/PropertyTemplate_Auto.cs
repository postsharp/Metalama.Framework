﻿// Ignored because not all declarations listed here are curretnly supported (the test should be be split into a few smaller ones).

using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Auto
{
    // Tests single OverrideProperty aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                var discard = meta.Proceed();
            }
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        public int Property { get; }

        [Override]
        private int PrivateProperty { get; }

        [Override]
        private int ProtectedProperty { get; }

        [Override]
        private protected int PrivateProtectedProperty { get; }

        [Override]
        protected internal int ProtectedInternalProperty { get; }

        [Override]
        protected internal int InternalProperty { get; }

        [Override]
        public int PropertyWithSetter { get; set; }

        [Override]
        public int PropertyWithRestrictedSetter { get; private set; }

        [Override]
        public int PropertyWithRestrictedGetter { private get; set; }

        [Override]
        public int PropertyWithInitSetter { get; init; }

        [Override]
        public int PropertyWithRestrictedInitSetter { get; protected init; }

        [Override]
        public int PropertyWithInitializer { get; set; } = 42;
    }
}
