using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33329;

// <target>
[CompileTime]
class C
{    [CompileTime]
    void M()
    {
        LocalFunction();
        StaticLocalFunction();

        void LocalFunction() { }
        static void StaticLocalFunction() { }
    }
}