using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Diagnostics.ReportFromTemplate
{
    public class LogAttribute : OverrideMethodAspect
    {
        
        public static Caravela.Framework.Diagnostics.DiagnosticDescriptor Error1 = new( "MY001", Caravela.Framework.Diagnostics.DiagnosticSeverity.Error, "Invalid method {0}." );
        public override dynamic OverrideMethod()
        {
            Error1.Report( target.Method );
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
