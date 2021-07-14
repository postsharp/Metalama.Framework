// @ReportOutputWarnings

#if !TESTRUNNER
// Disable the warning in the main build, not during tests.
#pragma warning disable CS0219
#endif

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.OverrideMethod
{
    public class SuppressWarningAttribute : Attribute, IAspect<INamedType>
    {
        private static readonly SuppressionDefinition _suppression1 = new( "CS0219" );

       
        
        
        [Template]
        public void Introduced()
        {
            int x = 0;
           
        }
        
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var introduced = builder.AdviceFactory.IntroduceMethod( builder.TargetDeclaration, nameof(Introduced));
            builder.Diagnostics.Suppress( introduced, _suppression1 );
        }
    }
    
    // <target>
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
