#pragma warning disable CS0067
#pragma warning disable CS0169

using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Xunit;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Introduce_BuildReflection_NotNull
{
    public class Introduction : TypeAspect
    {
        // Events
        [Introduce]
        public event EventHandler? EventField;

        // Public fields.
        [Introduce]
        public int IntroducedField;

        [Introduce]
        public int IntroducedField_Initializer = 42;

        [Introduce]
        public static int IntroducedField_Static;

        [Introduce]
        public static int IntroducedField_Static_Initializer = 42;

        // Private fields.
        [Introduce]
        private int IntroducedField_Private;

        [Introduce]
        private int IntroducedField_Initializer_Private = 42;

        [Introduce]
        private static int IntroducedField_Static_Private;

        // Public methods.
        [Introduce]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine("This is introduced method.");
            Console.WriteLine(this.IntroducedField_Initializer_Private);
            meta.Proceed();
        }

        [Introduce]
        public int IntroducedMethod_Int()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }

        [Introduce]
        public int IntroducedMethod_Param(int x)
        {
            Console.WriteLine($"This is introduced method, x = {x}.");

            return meta.Proceed();
        }

        [Introduce]
        public static int IntroducedMethod_StaticSignature()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }

        [Introduce(IsVirtual = true)]
        public int IntroducedMethod_VirtualExplicit()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }

        // Private methods.
        [Introduce]
        private void IntroducedMethod_Void_Private()
        {
            Console.WriteLine("This is introduced method.");
            meta.Proceed();
        }

        [Introduce]
        private int IntroducedMethod_Int_Private()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }

        [Introduce]
        private int IntroducedMethod_Param_Private(int x)
        {
            Console.WriteLine($"This is introduced method, x = {x}.");

            return meta.Proceed();
        }

        [Introduce]
        private static int IntroducedMethod_StaticSignature_Private()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }

        // Generic methods
        [Introduce]
        public T GenericMethod<T>(T a)
        {
            return a;
        }

        // Methods with modifiers.
        [Introduce]
        public void OutMethod( out int x)
        {
            x = 42;
            Console.WriteLine("OutMethod with parameter.");
        }

        [Introduce]
        public int RefMethod(ref int x)
        {
            x += 42;
            return 42;
        }

        // Public properties.
        [Introduce]
        public int IntroducedProperty_Auto { get; set; }

        [Introduce]
        public int IntroducedProperty_Auto_Initializer { get; set; } = 42;

        [Introduce]
        public int IntroducedProperty_Auto_GetOnly { get; }

        [Introduce]
        public int IntroducedProperty_Auto_GetOnly_Initializer { get; } = 42;

        [Introduce]
        public static int IntroducedProperty_Auto_Static { get; set; }

        [Introduce]
        public int IntroducedProperty_Accessors
        {
            get
            {
                Console.WriteLine("Get");

                return 42;
            }

            set
            {
                Console.WriteLine(value);
            }
        }

        // Private properties.
        [Introduce]
        private int IntroducedProperty_Auto_Private { get; set; }

        [Introduce]
        private int IntroducedProperty_Auto_Initializer_Private { get; set; } = 42;

        [Introduce]
        private int IntroducedProperty_Auto_GetOnly_Private { get; }

        [Introduce]
        private int IntroducedProperty_Auto_GetOnly_Initializer_Private { get; } = 42;

        [Introduce]
        private static int IntroducedProperty_Auto_Static_Private { get; set; }

        [Introduce]
        private int IntroducedProperty_Accessors_Private
        {
            get
            {
                Console.WriteLine("Get");

                return 42;
            }

            set
            {
                Console.WriteLine(value);
            }
        }
    }

    //<target>
    [Introduction]
    internal class Target { }

    public static class Program {

        public static void Main()
        {
            var target = new Target();

            // Events
            Assert.NotNull(target.GetType().GetEvent("EventField"));

            // Fields
            Assert.NotNull(target.GetType().GetField("IntroducedField"));
            Assert.NotNull(target.GetType().GetField("IntroducedField_Initializer"));
            Assert.NotNull(target.GetType().GetField("IntroducedField_Static"));
            Assert.NotNull(target.GetType().GetField("IntroducedField_Static_Initializer"));
            Assert.NotNull(target.GetType().GetField("IntroducedField_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetField("IntroducedField_Initializer_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetField("IntroducedField_Static_Private", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));

            // Methods
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_Void"));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_Int"));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_Param", new[] { typeof(Int32) }));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_StaticSignature"));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_VirtualExplicit"));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_Void_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_Int_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_Param_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetMethod("IntroducedMethod_StaticSignature_Private", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetMethod("GenericMethod"));
            Assert.NotNull(target.GetType().GetMethod("OutMethod", new[] { typeof(Int32).MakeByRefType() }));
            Assert.NotNull(target.GetType().GetMethod("RefMethod", new[] { typeof(Int32).MakeByRefType() }));

            // Properties
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto"));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_Initializer"));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_GetOnly"));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_GetOnly_Initializer"));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_Static"));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Accessors"));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_Initializer_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_GetOnly_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_GetOnly_Initializer_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Auto_Static_Private", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            Assert.NotNull(target.GetType().GetProperty("IntroducedProperty_Accessors_Private", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
        }
    }
}