using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.InvalidTarget
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
       

       
        
            }

    // <target>
    internal class TargetClass
    {
        [Introduction]
        void Method()
        {
        }
    }
}
