using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Aspects.UnitTests.Diagnostics.ReportFromInitialize
{
    public class ErrorAttribute : Attribute, IAspect<IMethod>
    {
        public static Caravela.Framework.Diagnostics.DiagnosticDescriptor Error1 = new( "MY001", Caravela.Framework.Diagnostics.DiagnosticSeverity.Error, "Invalid method {0}." );
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
            Error1.Report( aspectBuilder.TargetDeclaration );
        }
    }

    #region Target
    internal class TargetClass
    {
        [Error]
        public static int Add( int a, int b )
        {
            if ( a == 0 )
                throw new ArgumentOutOfRangeException( nameof( a ) );
            return a + b;
        }
    }
    #endregion
}
