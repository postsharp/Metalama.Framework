using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.TestFramework.Introduction;

public class C
{
    private void M()
    {
        #if METALAMA
        this.IntroducedMethod();
        #endif
    }

    class Fabric : TypeFabric
    {
        [Introduce]
        public void IntroducedMethod() { }
    }
}