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

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.OverrideMethod
{
    public class SuppressWarningAttribute : Attribute, IAspect<INamedType>
    {
        private string code;
        
        public SuppressWarningAttribute( string code )
        {
            this.code = code;
        }
        
        [IntroduceMethodTemplateAttribute]
        public void Introduced()
        {
            int x = 0;
           
        }
        
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            var introduced = aspectBuilder.AdviceFactory.IntroduceMethod( aspectBuilder.TargetDeclaration, nameof(Introduced));
            aspectBuilder.SuppressDiagnostic( this.code, introduced.Builder );
        }
    }
    
    [TestOutput]
    [SuppressWarning("CS0219")]
    internal class TargetClass
    {
        
        // CS0219 expected 
        private void M1( string m ) 
        {
           int x = 0;
        }
    }
}
