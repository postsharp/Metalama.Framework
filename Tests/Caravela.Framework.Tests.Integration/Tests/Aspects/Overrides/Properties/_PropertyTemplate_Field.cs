// @Skipped

// Ignored because not all declarations listed here are curretnly supported (the test should be be split into a few smaller ones).

using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Field
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

    // <target>
    internal class TargetClass
    {
        [Override]
        int _field;

        [Override]
        private int _privateField;

        [Override]
        private protected int PrivateProtectedField;

        [Override]
        protected int ProtectedField;

        [Override]
        protected internal int ProtectedInternalField;

        [Override]
        internal int InternalField;

        [Override]
        public int PublicField;

        [Override]
        private int _initializerField = 42;

        [Override]
        private static int _static_Field;

        [Override]
        private static int _static_InitializerField = 42;

        // Needs to change accesses in ctors to the newly defined backing field.
        // Linker needs to rewrite ctor bodies if there is any such field.
        // We cannot use init-only accessor (because that would make it usable from outside).

        // [Override]
        // private readonly int _readOnlyField;

        // [Override]
        // private readonly int _static_ReadOnlyField;

        // [Override]
        // private readonly int _initializerReadOnlyField = 42;

        // [Override]
        // private static readonly int _static_InitializerReadOnlyField = 42;

        static TargetClass()
        {
            // _static_ReadOnlyField = 42;
            // _static_InitializerReadOnlyField = 27;
        }

        public TargetClass()
        {
            // this._readOnlyField = 42;
            // this._initializerReadOnlyField = 27;
        }
    }
}
