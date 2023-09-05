using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_Parameters_Specialization;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");

        var templates1 = new BaseProvider<IMethod>();
        templates1.NotOverriddenTemplate(meta.Target.Method);
        templates1.OverriddenTemplate(meta.Target.Method);

        var templates2 = new DerivedProvider();
        templates2.NotOverriddenTemplate(meta.Target.Method);
        templates2.OverriddenTemplate(meta.Target.Method);

        return meta.Proceed();
    }
}

class BaseProvider<[CompileTime] T> : ITemplateProvider
{
    [Template]
    public virtual void OverriddenTemplate(T x)
    {
        Console.WriteLine($"called template x={x}");
    }

    [Template]
    public virtual void NotOverriddenTemplate(T x)
    {
        Console.WriteLine($"called template x={x}");
    }
}

class DerivedProvider : BaseProvider<IMethod>
{
    public override void OverriddenTemplate(IMethod x)
    {
        Console.WriteLine($"derived template x={x}");
    }
}

// <target>
class TargetCode
{
    [Aspect]
    void Method1()
    {
    }
}