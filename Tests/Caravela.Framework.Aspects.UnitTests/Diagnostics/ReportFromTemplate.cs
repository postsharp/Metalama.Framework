using Caravela.Framework.Diagnostics;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Diagnostics.ReportFromTemplate
{
    public class LogAttribute : OverrideMethodAspect
    {
        
        public override dynamic OverrideMethod()
        {
            target.ReportDiagnostic( Caravela.Framework.Diagnostics.Severity.Error, "MY001", "Invalid method." );

            return proceed();
        }
    }

    #region Target
    internal class TargetClass
    {
        [Log]
        public static int Add( int a, int b )
        {
            if ( a == 0 )
                throw new ArgumentOutOfRangeException( nameof( a ) );
            return a + b;
        }
    }
    #endregion
}
