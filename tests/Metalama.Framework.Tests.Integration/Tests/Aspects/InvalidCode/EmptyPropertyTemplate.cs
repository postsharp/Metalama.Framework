using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.InvalidCode.EmptyPropertyTemplate;

class Aspect : TypeAspect
{
#if TESTRUNNER
    [Template]
    public object Instance { }    
#endif
}