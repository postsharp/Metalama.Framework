
using System;
using Castle.DynamicProxy.Generators;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33509;

public class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Double, ConversionKind.Implicit)}");
        Console.WriteLine($"Should be false: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Double, ConversionKind.Default)}");
        Console.WriteLine($"Should be false: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Double, ConversionKind.Reference)}");
        Console.WriteLine($"Should be false: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Double, ConversionKind.TypeDefinition)}");

        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Object, ConversionKind.Implicit)}");
        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Object, ConversionKind.Default)}");
        Console.WriteLine($"Should be false: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Object, ConversionKind.Reference)}");
        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Int32).Is(SpecialType.Object, ConversionKind.TypeDefinition)}");

        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Task).Is(SpecialType.Object, ConversionKind.Implicit)}");
        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Task).Is(SpecialType.Object, ConversionKind.Default)}");
        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Task).Is(SpecialType.Object, ConversionKind.Reference)}");
        Console.WriteLine($"Should be true: {TypeFactory.GetType(SpecialType.Task).Is(SpecialType.Object, ConversionKind.TypeDefinition)}");

        return meta.Proceed();

    }
}

// <target>
public partial class TestClass
{
    [TestAspect]
    public void Foo()
    {
    }
}

