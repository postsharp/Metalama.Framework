using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Linq;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    public class CountMethodsAspect : Attribute, IAspect<INamedType>
    {
        int methodCount;

        [OverrideMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine( $"This is just 1 of {this.methodCount} methods." );
            return proceed();
        }

        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            var methods = aspectBuilder.TargetDeclaration.Methods.GetValue();
            this.methodCount = methods.Count();
            foreach ( var method in methods )
            {
                aspectBuilder.AdviceFactory.OverrideMethod( method, "Template" );
            }
        }
    }
}
