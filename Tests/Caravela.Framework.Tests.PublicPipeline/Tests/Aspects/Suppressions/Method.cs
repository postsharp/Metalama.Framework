// @ReportOutputWarnings
// @ClearIgnoredDiagnostics

#if !TESTRUNNER
// Disable the warning in the main build, not during tests.
#pragma warning disable CS0219
#endif



using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.Methods
{
    public class SuppressWarningAttribute : Attribute, IAspect<IMethod>
    {
        private static readonly SuppressionDefinition _suppression1 = new( "CS0219" );

       
        
        
        public void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Diagnostics.Suppress( null, _suppression1 );
        }
    }
    
    // <target>
    internal class TargetClass
    {
        [SuppressWarning]
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

