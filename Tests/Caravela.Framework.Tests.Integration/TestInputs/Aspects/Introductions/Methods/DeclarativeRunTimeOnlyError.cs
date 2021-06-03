using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.DeclarativeRunTimeOnlyError
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
    
        [Introduce]
        public virtual void IntroducedMethodWithParam( RunTimeOnlyClass p )
        {
        }

        [Introduce]
        public virtual RunTimeOnlyClass? IntroducedMethodWithRet( )
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
