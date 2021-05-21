// @IncludeFinalDiagnostics

#if !TESTRUNNER
// Disable the warning in the main build, not during tests.
#pragma warning disable CS0169
#endif

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Suppressions.Fields
{
    public class SuppressWarningAttribute : Attribute, IAspect<IField>
    {
        private static readonly SuppressionDefinition _suppression1 = new( "CS0169" );
        private static readonly SuppressionDefinition _suppression2 = new( "CS0649" );
        
        public SuppressWarningAttribute()
        {
        }
        
        public void Initialize(IAspectBuilder<IField> aspectBuilder)
        {
            aspectBuilder.Diagnostics.Suppress( null, _suppression1 );
            aspectBuilder.Diagnostics.Suppress( null, _suppression2 );
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        
        // CS0169 expected here.
        int x;
        
        [SuppressWarning]
        int y;
        
        [SuppressWarning]
        int w, z;
        
        
    }
}
