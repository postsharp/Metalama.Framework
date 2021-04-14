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
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.NestedScopes
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
            return proceed();
        }
        
        public void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.OverrideMethod( aspectBuilder.TargetDeclaration, nameof(Override), AspectLinkerOptions.Create(true) );
            aspectBuilder.SuppressDiagnostic( this.code, aspectBuilder.TargetDeclaration );
        }
    }
    
    [TestOutput]
    internal class TargetClass
    {
        [SuppressWarning("CS0219")]
        private void M2( string m ) 
        {
           #pragma warning disable CS0219
           int x = 0;
           #pragma warning restore CS0219
           
           int y = 0;
        }
        
        
        private void M1( string m ) 
        {
           #pragma warning disable CS0219
           int x = 0;
           #pragma warning restore CS0219
        
           // CS0219 expected 
           int y = 0;
        }
    }
}
