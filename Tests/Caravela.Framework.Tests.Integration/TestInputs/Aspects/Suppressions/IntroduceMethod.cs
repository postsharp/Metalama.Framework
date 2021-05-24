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
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.OverrideMethod
{
    public class SuppressWarningAttribute : Attribute, IAspect<INamedType>
    {
        private static readonly SuppressionDefinition _suppression1 = new( "CS0219" );

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder) { }

        [IntroduceMethodTemplateAttribute]
        public void Introduced()
        {
            int x = 0;
           
        }
        
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var introduced = builder.AdviceFactory.IntroduceMethod( builder.TargetDeclaration, nameof(Introduced));
            builder.Diagnostics.Suppress( introduced.Builder, _suppression1 );
        }
    }
    
    [TestOutput]
    [SuppressWarning]
    internal class TargetClass
    {
        
        // CS0219 expected 
        private void M1( string m ) 
        {
           int x = 0;
        }
    }
}
