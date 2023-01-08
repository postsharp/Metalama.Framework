using System;
using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.ComplexFlow_ForcedJumps;

[assembly:AspectOrder(typeof(OuterAspect), typeof(InnerAspect))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.ComplexFlow_ForcedJumps;

/*
 * Verifies that inlining with forced jumps into a local function produces correct code.
 */

public class OuterAspect : OverrideMethodAspect
{    
    public override dynamic? OverrideMethod()
    {
        int OuterLocalFunction()
        {
            if (meta.Target.Parameters[0].Value == 27)
            {
                meta.InsertComment("The outer method is inlining into the middle of the method.");
                meta.Proceed();
            }

            Console.WriteLine("Outer");

            return 27;
        }

        return OuterLocalFunction();
    }
}

public class InnerAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        int InnerLocalFunction()
        {
            if (meta.Target.Parameters[0].Value == 42)
            {
                meta.InsertComment("The inliner is replacing local declaration, i.e. return replacements need to be used.");
                meta.InsertComment("All branches of this if statement need to return from the local function.");
                var x = meta.Proceed();
                Console.WriteLine("Inner");
                return x;
            }

            Console.WriteLine("Inner");

            return 42;
        }

        return InnerLocalFunction();
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
            // The inlined body has a return from the middle.
            return 27;
        }

        Console.WriteLine("Original");

        return 42;
    }
}