﻿using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.Hierarchy;
using System;

#pragma warning disable CS0067

[assembly: AspectOrder(AspectOrderDirection.CompileTime, typeof(IntroductionAttribute), typeof(TestAttribute))]

namespace Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.Hierarchy
{
    /*
     * Tests a that hierarchical interfaces are implemented and the implementation is visible in code model.
     */

    public interface IBase0Interface
    {
        void Foo();
    }

    public interface IBase1Interface
    {
        void Goo();
    }

    public interface IBase2Interface : IBase0Interface, IBase1Interface
    {
        void Zoo();
    }

    public interface IBase3Interface<T>
    {
        void Bar();
    }

    public interface IInterface : IBase2Interface, IBase3Interface<int>
    {
        void Quz();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.ImplementInterface(typeof(IInterface), whenExists: OverrideStrategy.Ignore);
        }

        [InterfaceMember]
        public void Foo()
        {
            Console.WriteLine("Introduced interface member");
        }

        [InterfaceMember]
        public void Goo()
        {
            Console.WriteLine("Introduced interface member");
        }

        [InterfaceMember]
        public void Zoo()
        {
            Console.WriteLine("Introduced interface member");
        }

        [InterfaceMember]
        public void Bar()
        {
            Console.WriteLine("Introduced interface member");
        }

        [InterfaceMember]
        public void Quz()
        {
            Console.WriteLine("Introduced interface member");
        }
    }

    public class TestAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.New)]
        public void Evaluator()
        {
            Console.WriteLine($"Target type implements IBase0Interface: {meta.Target.Type.Is(typeof(IBase0Interface))}");
            Console.WriteLine($"Target type implements IBase1Interface: {meta.Target.Type.Is(typeof(IBase1Interface))}");
            Console.WriteLine($"Target type implements IBase2Interface: {meta.Target.Type.Is(typeof(IBase2Interface))}");
            Console.WriteLine($"Target type implements IBase3Interface<int>: {meta.Target.Type.Is(typeof(IBase3Interface<int>))}");
            Console.WriteLine($"Target type implements IInterface: {meta.Target.Type.Is(typeof(IInterface))}");

            foreach (var implementedInterface in meta.Target.Type.ImplementedInterfaces)
            {
                Console.WriteLine($"ImplementedInterfaces contains {implementedInterface}.");
            }

            foreach (var implementedInterface in meta.Target.Type.AllImplementedInterfaces)
            {
                Console.WriteLine($"AllImplementedInterfaces constains {implementedInterface}.");
            }
        }
    }

    // <target>
    [Introduction]
    [Test]
    public class TargetClass : IBase0Interface 
    {
        public void Foo()
        {
            Console.WriteLine("Original interface member");
        }
    }

    // <target>
    [Test]
    public class DerivedClass : TargetClass { }
}