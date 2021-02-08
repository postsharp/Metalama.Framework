#pragma warning disable SA1649 // File name should match first type name

using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Samples.SimpleLogging
{
    public class LogAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine( target.Method.ToDisplayString() + " started." );

            try
            {
                dynamic result = proceed();

                Console.WriteLine( target.Method.ToDisplayString() + " succeeded." );
                return result;
            }
            catch ( Exception e )
            {
                Console.WriteLine( target.Method.ToDisplayString() + " failed: " + e.Message );
                throw;
            }
        }
    }

    #region Target
    internal class TargetClass
    {
        [Log]
        public static int Add( int a, int b )
        {
            if ( a == 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( a ) );
            }

            return a + b;
        }
    }
    #endregion
}
