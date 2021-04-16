// @IncludeFinalDiagnostics


#if !TESTRUNNER
// Disable the warning in the main build, not during tests.
#pragma warning disable CS0219
#endif



using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.Methods
{
    public class SuppressWarningAttribute : Attribute, IAspect<IMethod>
    {
        private string code;
        
        public SuppressWarningAttribute( string code )
        {
            this.code = code;
        }
        
        
        public void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.SuppressDiagnostic( this.code, aspectBuilder.TargetDeclaration );
        }
    }
    
    [TestOutput]
    internal class TargetClass
    {
        [SuppressWarning("CS0219")]
        private void M2( string m ) 
        {
           int x = 0;
        }
        
        // CS0219 expected 
        private void M1( string m ) 
        {
           int x = 0;
        }
    }
}

