using System;
using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.ComplexFlow;

[assembly:AspectOrder(typeof(OuterAspect), typeof(InnerAspect))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.ComplexFlow;

public class OuterAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        int LocalFunction()
        {
            if (meta.Target.Parameters[0].Value == 27)
            {
                meta.Proceed();
                return 42;
            }

            Console.WriteLine("Outer");

            return 27;
        }

        if (meta.Target.Parameters[0].Value == 27)
        {
            return 42;
        }

        Console.WriteLine("Outer");

        return LocalFunction();
    }
}

public class InnerAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        int LocalFunction()
        {
            if (meta.Target.Parameters[0].Value == 27)
            {
                meta.Proceed();
                return 42;
            }

            Console.WriteLine("Inner");

            return 27;
        }

        if (meta.Target.Parameters[0].Value == 27)
        {
            return 42;
        }

        Console.WriteLine("Inner");

        return LocalFunction();
    }
}

// <target>
internal class TargetClass
{
    [OuterAspect]
    [InnerAspect]
    private int Method(int z)
    {
        if (z == 42)
        {
            return 27;
        }

        Console.WriteLine("Original");

        return 42;
    }
}