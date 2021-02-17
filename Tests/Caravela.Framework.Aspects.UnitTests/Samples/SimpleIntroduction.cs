using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Samples.SimpleIntroduction
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
        }

        [IntroduceMethod]
        public void IntroducedMethod()
        {
            Console.WriteLine( "This is introduced method." );
            proceed();
        }
    }

    #region Target
    [Introduction]
    internal class TargetClass
    {
    }
    #endregion
}
