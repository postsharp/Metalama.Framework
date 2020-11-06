using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp
{
    public class SwallowExceptionsAspect : OverrideMethodAspect
    {
        [OverrideMethodTemplate]
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
