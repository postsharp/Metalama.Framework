// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.CodeFixes;
using System.ComponentModel;

namespace Caravela.Framework.Tests.Integration.CodeFixes.AddAttribute
{
    class Aspect : MethodAspect
    {
        static DiagnosticDefinition _diag = new DiagnosticDefinition( "MY001", Severity.Warning, "Add some attribute" );
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            builder.Diagnostics.Report( _diag, CodeFix.AddAttribute(  builder.Target, typeof(MyAttribute) ) );
        }
    }
    
    class MyAttribute : Attribute {}

    class TargetCode
    {
        [Aspect]
        [My]
        int Method(int a)
        {
            return a;
        }
    }
}
