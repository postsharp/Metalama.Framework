using System;
using Caravela.Framework.Aspects;


namespace Caravela.Framework.TestApp.Aspects
{
    public class SwallowExceptionsAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            try
            {
                return meta.Proceed();
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "Caravela caught: " + ex );
                return default;
            }
        }
    }
}
