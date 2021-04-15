// @IncludeFinalDiagnostics

#if !TESTRUNNER
// Disable the warning in the main build, not during tests.
#pragma warning disable CS0169
#endif

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.Fields
{
    public class SuppressWarningAttribute : Attribute, IAspect<IField>
    {
        private string[] codes;
        
        public SuppressWarningAttribute( params string[] codes )
        {
            this.codes = codes;
        }
        
        public void Initialize(IAspectBuilder<IField> aspectBuilder)
        {
            foreach ( var code in codes )
            {
                aspectBuilder.SuppressDiagnostic( code, aspectBuilder.TargetDeclaration );
            }
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        
        // CS0169 expected here.
        int x;
        
        [SuppressWarning("CS0169", "CS0649")]
        int y;
        
        [SuppressWarning("CS0169", "CS0649")]
        int w, z;
        
        
    }
}
