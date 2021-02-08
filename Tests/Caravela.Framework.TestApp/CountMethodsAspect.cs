using System;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    public class CountMethodsAspect : Attribute, IAspect<INamedType>
    {
        private int _i;
        private int _methodCount;

        [OverrideMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine( "Hello, world." );
            Console.WriteLine( $"This is {++this._i} of {this._methodCount} methods." );
            return proceed();
        }

        public void Initialize( IAspectBuilder<INamedType> aspectBuilder )
        {
            var methods = aspectBuilder.TargetDeclaration.Methods.GetValue();
            this._methodCount = methods.Count();
            foreach ( var method in methods.Where( x => x.MethodKind != Caravela.Framework.Code.MethodKind.Constructor ) )
            {
                aspectBuilder.AdviceFactory.OverrideMethod( method, nameof( this.Template ) );
            }
        }
    }
}
