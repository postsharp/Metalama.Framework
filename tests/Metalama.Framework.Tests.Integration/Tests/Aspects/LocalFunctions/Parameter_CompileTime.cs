using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LocalFunctions.Parameter_CompileTime;

class Aspect : TypeAspect
{
    [Template]
    void M()
    {
        LogMethod(null);
        LogString("foo");

        void LogMethod(IMethod? instance) => Console.WriteLine(instance?.ToString());
        void LogString([CompileTime] string instance) => Console.WriteLine(instance);
    }

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceMethod(builder.Target, nameof(M));
    }
}

// <target>
[Aspect]
class C { }