// @DesignTime

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.DesignTimeNonPartial
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine("This method should not be introduced in design time because the target class is not partial.");
            var nic = meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}
