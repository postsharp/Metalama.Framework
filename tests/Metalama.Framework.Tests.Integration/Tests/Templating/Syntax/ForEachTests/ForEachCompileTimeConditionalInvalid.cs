using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachCompileTimeConditionalInvalid;

#pragma warning disable CS0169

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        var fieldName = meta.CompileTime("");

        if (meta.This.logMembers)
        {
            foreach (var field in meta.Target.Type.FieldsAndProperties)
            {
                fieldName = field.Name;
            }
        }

        Console.WriteLine(fieldName);

        return meta.Proceed();
    }
}

class TargetCode
{
    bool logMembers;

    void Method() { }
}