// @IncludeFinalDiagnostics

#if !TESTRUNNER
// Disable the warning in the main build, not during tests.
#pragma warning disable CS0219
#endif


using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Advices;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.IntroduceMethod
{
    public class SuppressWarningAttribute : Attribute, IAspect<IMethod>
    {
        private string code;
        
        public SuppressWarningAttribute( string code )
        {
            this.code = code;
        }
        
        [OverrideMethodTemplateAttribute]
        public dynamic Override()
        {
            int a = 0;
            return meta.Proceed();
        }
        
        public void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.OverrideMethod( aspectBuilder.TargetDeclaration, nameof(Override), AspectLinkerOptions.Create(true) );
            aspectBuilder.Diagnostics.Suppress( this.code, aspectBuilder.TargetDeclaration );
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
