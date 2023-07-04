using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LocalFunctions.Parameter;

class Aspect : MethodAspect
{
    [Template]
    public Func<object?, object?[], object?> GetOriginalMethodInvoker(IMethod method)
    {
        return Invoke;

        object? Invoke(object? instance, object?[] args)
        {
            if (method.IsStatic)
            {
                return method.Invoke(args[0]!);
            }
            else
            {
                return method.With(instance).Invoke(args[0]!);
            }
        }
    }
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.IntroduceMethod(
            builder.Target.DeclaringType,
            nameof(GetOriginalMethodInvoker),
            args: new { method = builder.Target });
    }
}

// <target>
class C
{
    [Aspect]
    int M(int i) => 42;
}