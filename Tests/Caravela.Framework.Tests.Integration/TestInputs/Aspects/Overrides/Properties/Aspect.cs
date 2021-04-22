using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using static Caravela.Framework.Aspects.TemplateContext;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Simple
{
    // Tests single OverrideProperty aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");
                return proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                proceed();
            }
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        private int _instanceField;

        [Override]
        private int _initializerField = 42;

        // Needs to change accesses in ctors to the newly defined backing field.
        // Linker needs to rewrite ctor bodies if there is any such field.
        [Override]
        private readonly int _readOnlyField;

        [Override]
        private readonly int _initializerReadOnlyField = 42;

        [Override]
        public int PublicInstanceField;

        [Override]
        public static int PublicStaticField;

        // Same as readonly field.
        [Override]
        public int AutoProperty { get; }

        [Override]
        public int AutoPropertyWithSetter { get; set; }

        [Override]
        public int AutoPropertyWithPrivateSetter { get; private set; }

        [Override]
        public int AutoPropertyWithInitSetter { get; init; }

        [Override]
        public int AutoPropertyWithInitializer { get; set; } = 42;

        [Override]
        public int ExpressionProperty => 42;

        [Override]
        public static int StaticProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int InstanceProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int InitProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }

            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int GetterProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
        }

        [Override]
        public int SetterProperty
        {
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int InitSetterProperty
        {
            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        protected int ProtectedProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int DifferentAccessibilityProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
            private set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        protected int ExpressionBodiedProperty
        {
            get => 42;
            set => Console.WriteLine($"This is the original setter, setting {value}.");
        }

        public TargetClass()
        {
            this._readOnlyField = 42;
            this._initializerReadOnlyField = 27;
            this.AutoProperty = 42;
        }
    }
}
