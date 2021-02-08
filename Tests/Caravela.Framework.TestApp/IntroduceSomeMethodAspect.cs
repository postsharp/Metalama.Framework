using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    public class IntroduceSomeMethodAspect : Attribute, IAspect<INamedType>
    {
        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.AdviceFactory.IntroduceMethod( aspectBuilder.TargetDeclaration, nameof( SomeIntroducedMethod ) );
        }

        [IntroduceMethodTemplate]
        public static void SomeIntroducedMethod()
        {
            Console.WriteLine( "From IntroduceSomeMethodAspect!" );

            proceed();
        }

        [IntroduceMethod]
        public void SomeOtherIntroducedMethod()
        {
        }
    }
}
