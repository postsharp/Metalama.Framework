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

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.IntroduceMethod
{
    public class SuppressWarningAttribute : Attribute, IAspect<IMethod>
    {
        private static readonly SuppressionDefinition _suppression = new( "CS0219" );
        
        public SuppressWarningAttribute()
        {
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
            aspectBuilder.Diagnostics.Suppress( null, _suppression );
        }
    }
    
    [TestOutput]
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
