using System;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Overrides.Methods.Programmatic
{
    public class OverrideAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            var advice = aspectBuilder.AdviceFactory.OverrideMethod( aspectBuilder.TargetDeclaration.Methods.OfName("TargetMethod").Single(), nameof( Template ) );
        }

        [OverrideMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine( "This is the overriding method." );
            return proceed();
        }
    }

    #region Target
    [Override]
    internal class TargetClass
    {
        public void TargetMethod()
        {
            Console.WriteLine( "This is the original method." );
        }
    }
    #endregion
}
