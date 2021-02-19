using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Aspects.UnitTests.Diagnostics.SkipWithoutError
{
    public class SkippedAttribute : OverrideMethodAspect
    {
        public override void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
            base.Initialize( aspectBuilder );
            
            aspectBuilder.SkipAspect();
        }

        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException("This code should not be emitted.");
        }
    }

    #region Target
    internal class TargetClass
    {
        [Skipped]
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
