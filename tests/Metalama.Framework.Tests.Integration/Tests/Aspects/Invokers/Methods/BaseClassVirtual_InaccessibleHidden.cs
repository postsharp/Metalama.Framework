using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_InaccessibleHidden;

public class InvokerAttribute : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        var targetMethod = meta.Target.Type.BaseType.BaseType.Methods.OfName("TargetMethod").SingleOrDefault();

        meta.InsertComment("Dynamic invokers");
        meta.This.TargetMethod("Dynamic-This");
        meta.Base.TargetMethod("Dynamic-Base");

        meta.InsertComment("Explicit invokers");
        targetMethod.Invoke("ImplicitDefault");
        targetMethod.With(InvokerOptions.NullConditional).Invoke("ImplicitDefault-NullConditional");
        targetMethod.With(InvokerOptions.Default).Invoke("Default");
        targetMethod.With(InvokerOptions.Default | InvokerOptions.NullConditional).Invoke("Default-NullConditional");
        targetMethod.With(InvokerOptions.Base).Invoke("Base");
        targetMethod.With(InvokerOptions.Base | InvokerOptions.NullConditional).Invoke("Base-NullConditional");
        targetMethod.With(InvokerOptions.Current).Invoke("Current");
        targetMethod.With(InvokerOptions.Current | InvokerOptions.NullConditional).Invoke("Current-NullConditional");
        targetMethod.With(InvokerOptions.Final).Invoke("Final");
        targetMethod.With(InvokerOptions.Final | InvokerOptions.NullConditional).Invoke("Final-NullConditional");

        meta.InsertComment("Another instance dynamic");
        meta.Target.Parameters[0].Value.TargetMethod("Dynamic-Parameter");

        meta.InsertComment("Another instance");
        targetMethod.With(meta.Target.Parameters[0]).Invoke("ImplicitDefault");
        targetMethod.With(meta.Target.Parameters[0], InvokerOptions.NullConditional).Invoke("ImplicitDefault-NullConditional");
        targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Default).Invoke("Default");
        targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Default | InvokerOptions.NullConditional).Invoke("Default-NullConditional");
        targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Final).Invoke("Final");
        targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Final | InvokerOptions.NullConditional).Invoke("Final-NullConditional");

        return meta.Proceed();
    }
}

public class BaseBaseClass
{
    public virtual void TargetMethod(string arg)
    {
        Console.WriteLine($"Inaccessible Hidden: {arg}");
    }
}

public class BaseClass : BaseBaseClass
{
    public new void TargetMethod(string arg)
    {
        Console.WriteLine($"Hidden: {arg}");
    }
}

// <target>
public class TargetClass : BaseClass
{
    [Invoker]
    public void InvokerMethod(TargetClass instance)
    {
    }
}
