using Caravela.Framework.Aspects;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.TestApp.Aspects
{
    public class SwallowExceptionsAspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            try
            {
                return proceed();
            }
            catch (Exception ex)
            {
                Console.WriteLine( "Caravela caught: " + ex );
                return default;
            }
        }
    }
}
