using Caravela.Framework.TestApp.Aspects;

namespace Caravela.Framework.TestApp
{
    partial class ClassWithInheritedAspect : IInterface
    {
        public void ManualMethod()
        {
            this.IntroducedMethod();
        }
    }
}
