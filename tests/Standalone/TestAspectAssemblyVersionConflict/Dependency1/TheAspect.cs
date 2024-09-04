using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

namespace Dependency;


[Inheritable]
public class TheAspect : TypeAspect
{
    [Introduce(WhenExists = OverrideStrategy.New)]
    public void IntroducedMethod1() { }
}