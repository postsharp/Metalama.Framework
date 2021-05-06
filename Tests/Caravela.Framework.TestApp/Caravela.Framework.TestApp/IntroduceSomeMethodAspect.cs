using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;


namespace Caravela.Framework.TestApp
{
    public class IntroduceSomeMethodAspect : Attribute, IAspect<INamedType>
    {
        string[] methodNames;

        public IntroduceSomeMethodAspect(params string[] methodNames)
        {
            this.methodNames = methodNames;
        }

        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            foreach ( string methodName in methodNames )
            {
                var advice = aspectBuilder.AdviceFactory.IntroduceMethod( aspectBuilder.TargetDeclaration, nameof( SomeIntroducedMethod ) );
                advice.Builder.Name = methodName;
            }
        }

        [IntroduceMethodTemplate]
        public static void SomeIntroducedMethod()
        {
            Console.WriteLine( "From IntroduceSomeMethodAspect!" );

            var x = meta.Proceed();
        }

        [IntroduceMethod]
        public void SomeOtherIntroducedMethod()
        {
            
        }

        [IntroduceMethod]
        public void SomeOtherIntroducedMethod5()
        {

        }
    }
}
