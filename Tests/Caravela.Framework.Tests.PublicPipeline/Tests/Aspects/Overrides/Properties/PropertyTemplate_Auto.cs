using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Auto
{
    // Tests single OverrideProperty aspect on auto properties.

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
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public int Property { get; }

        [Override]
        public static int Static_Property { get; }

        [Override]
        private int PrivateProperty { get; }

        [Override]
        protected int ProtectedProperty { get; }

        [Override]
        private protected int PrivateProtectedProperty { get; }

        [Override]
        protected internal int ProtectedInternalProperty { get; }

        [Override]
        protected internal int InternalProperty { get; }

        [Override]
        public int PropertyWithSetter { get; set; }

        [Override]
        public static int Static_PropertyWithSetter { get; set; }

        [Override]
        public int PropertyWithRestrictedSetter { get; private set; }

        [Override]
        public int PropertyWithRestrictedGetter { private get; set; }

        [Override]
        public int PropertyWithInitSetter { get; init; }

        [Override]
        public int PropertyWithRestrictedInitSetter { get; protected init; }

        // Needs to change accesses in ctors to the newly defined backing field.
        // Linker needs to rewrite ctor bodies if there is any such field.

        //[Override]
        //public int GetterPropertyWithInitializer { get; } = 42;

        //[Override]
        //public static int Static_GetterPropertyWithInitializer { get; } = 42;

        //[Override]
        //public int PropertyWithInitializer { get; set; } = 42;

        //[Override]
        //public static int Static_PropertyWithInitializer { get; set; } = 42;
    }
}
