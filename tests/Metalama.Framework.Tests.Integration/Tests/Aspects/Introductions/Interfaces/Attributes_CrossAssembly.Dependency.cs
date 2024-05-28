﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes_CrossAssembly
{
    public class TestInterfaceAttribute : Attribute 
    { 
        public TestInterfaceAttribute(string? value = null) { }
    }

    public class TestAspectAttribute : Attribute
    {
        public TestAspectAttribute(string? value = null) { }
    }

    public interface IInterface
    {
        [TestInterface]
        void Method();

        [TestInterface]
        int Property { [TestInterface("Getter")] get; [TestInterface("Setter")] set; }

        [TestInterface]
        int AutoProperty { [TestInterface("Getter")] get; [TestInterface("Setter")] set; }

        [TestInterface]
        event EventHandler? EventField;

        [TestInterface]
        event EventHandler? Event;
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advice.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
        }

        [Introduce]
        [TestAspect]
        public void Method()
        {
            Console.WriteLine("Introduced interface member");
        }

        [Introduce]
        [TestAspect]
        public int Property
        {
            [TestAspect("Getter")]
            get
            {
                return 42;
            }

            [TestAspect("Setter")]
            set
            {
            }
        }

        [Introduce]
        [TestAspect]
        public int AutoProperty
        {
            [TestAspect("Getter")]
            get;

            [TestAspect("Setter")]
            set;
        }

        [Introduce]
        [TestAspect]
        public event EventHandler? EventField;

        [Introduce]
        [TestAspect]
        public event EventHandler? Event
        {
            [TestAspect("Adder")]
            add { }

            [TestAspect("Remover")]
            remove { }
        }
    }
}