using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LocalFunctions.Parameter_ArgsArray;

class Aspect : MethodAspect
{
    [Template]
    public Func<object?, object?[], object?> GetMethodInvoker(IMethod method)
    {
        return Invoke;

        object? Invoke(object? instance, object?[] args)
        {
            return method.With(instance).Invoke(args[0]!);
        }
    }

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.IntroduceMethod(
            builder.Target.DeclaringType,
            nameof(GetMethodInvoker),
            args: new { method = builder.Target });
    }
}

// <target>
class C
{
    [Aspect]
    int M(int i) => 42;
}