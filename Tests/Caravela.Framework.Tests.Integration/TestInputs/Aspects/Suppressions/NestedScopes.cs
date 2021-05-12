// @IncludeFinalDiagnostics



using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Advices;

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
#if !TESTRUNNER // Disable the warning in the main build, not during tests. (1)
#pragma warning disable CS0219
#endif            
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
#pragma warning disable CS0219
           int x = 0;
#pragma warning restore CS0219


#if !TESTRUNNER // Disable the warning in the main build, not during tests. (1)
#pragma warning disable CS0219
#endif

            int y = 0;
        }
        
        
        private void M1( string m ) 
        {
#pragma warning disable CS0219
           int x = 0;
#pragma warning restore CS0219

#if !TESTRUNNER // Disable the warning in the main build, not during tests. (2)
#pragma warning disable CS0219, 219
#endif


            // CS0219 expected 
            int y = 0;
        }
    }
}
