using Metalama.Framework.TestApp.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.TestApp
{
    partial class ClassWithInheritedAspect : IInterface
    {
        public void ManualMethod()
        {
            this.IntroducedMethod();
        }
    }


}
