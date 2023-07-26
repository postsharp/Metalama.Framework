using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LocalFunctions.Parameter_Simple;

class Aspect : TypeAspect
{
    [Template]
    void M()
    {
        Log("foo");

        void Log(string instance) => Console.WriteLine(instance);
    }

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceMethod(builder.Target, nameof(M));
    }
}

// <target>
[Aspect]
class C { }