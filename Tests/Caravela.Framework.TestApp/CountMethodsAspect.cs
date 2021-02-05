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
        int i;
        int methodCount;

        [OverrideMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine( "Hello, world." );
            Console.WriteLine( $"This is {++this.i} of {this.methodCount} methods." );
            return proceed();
        }

        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            var methods = aspectBuilder.TargetDeclaration.Methods.GetValue();
            this.methodCount = methods.Count();
            foreach ( var method in methods )
            {
                aspectBuilder.AdviceFactory.OverrideMethod( method, nameof(this.Template ) );
            }
        }
    }
}
