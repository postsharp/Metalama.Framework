using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    // TODO: provide some base classes to remove the AttributeUsage boilerplate?
    [AttributeUsage(AttributeTargets.Method)]
    public class SwallowExceptionsAspect : Attribute, IAspect<IMethod>
    {
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder ) { }

        [OverrideMethod]
        public dynamic Template()
        {
            try
            {
                return proceed();
            }
            catch (Exception ex)
            {
                Console.WriteLine( "Caravela caught: " + ex );
                return null;
            }
        }
    }
}
