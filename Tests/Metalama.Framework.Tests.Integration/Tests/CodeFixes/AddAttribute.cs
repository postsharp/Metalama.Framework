// @ApplyCodeFix

using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.CodeFixes;
using System.ComponentModel;

namespace Metalama.Framework.Tests.Integration.CodeFixes.AddAttribute
{
    class Aspect : MethodAspect
    {
        static DiagnosticDefinition _diag = new ( "MY001", Severity.Warning, "Add some attribute" );
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            _diag.WithCodeFixes( CodeFix.AddAttribute(  builder.Target, typeof(MyAttribute) )  ).ReportTo( builder.Diagnostics );
        }
    }
    
    class MyAttribute : Attribute {}

    class TargetCode
    {
        [Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}