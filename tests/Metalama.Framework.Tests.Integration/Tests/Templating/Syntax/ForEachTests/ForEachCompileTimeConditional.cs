using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachCompileTimeConditional;

#pragma warning disable CS0169

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        if (meta.This.logMembers)
        {
            foreach (var field in meta.Target.Type.FieldsAndProperties)
            {
                Console.WriteLine($"{field.Name} = {field.Value}");
            }
        }

        return meta.Proceed();
    }
}

class TargetCode
{
    bool logMembers;

    int Method(int a, int bb)
    {
        return a + bb;
    }
}