using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.DeclarativeRunTimeOnly
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
    
        [Introduce]
        public void IntroducedMethodWithParam( RunTimeOnlyClass p )
        {
        }

        [Introduce]
        public RunTimeOnlyClass? IntroducedMethodWithRet( )
        {
            return null;
        }

    }
    
    public class RunTimeOnlyClass {}

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
