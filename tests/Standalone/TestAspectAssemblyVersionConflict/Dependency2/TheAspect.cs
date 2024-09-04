using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Dependency;

[Inheritable]
public class TheAspect : TypeAspect
{
    [Introduce(WhenExists = OverrideStrategy.New)]
    public void IntroducedMethod2() { }
}