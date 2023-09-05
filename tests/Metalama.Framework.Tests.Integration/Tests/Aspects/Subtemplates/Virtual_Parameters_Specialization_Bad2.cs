using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_Parameters_Specialization_Bad2;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");

        new DerivedProvider().CalledTemplate(meta.This);

        return meta.Proceed();
    }
}

class BaseProvider<[CompileTime] T> : ITemplateProvider
{
    [Template]
    public virtual void CalledTemplate(T x)
    {
        Console.WriteLine($"called template x={x}");
    }
}

class DerivedProvider : BaseProvider<TargetCode>
{
}

// <target>
class TargetCode
{
    [Aspect]
    void Method()
    {
    }
}