// Warning MY001 on `Method`: `Apply Aspect2`
//    CodeFix: Apply`
using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.CodeFixes;
using System.ComponentModel;
using Metalama.Framework.Tests.Integration.CodeFixes.ApplyAspect;

namespace Metalama.Framework.Tests.Integration.CodeFixes.ApplyAspect
{
    class Aspect1 : MethodAspect
    {
        static DiagnosticDefinition _diag = new ( "MY001", Severity.Warning, "Apply Aspect2" );
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            _diag.WithCodeFixes( CodeFix.ApplyAspect( builder.Target, new Aspect2(), "Apply" ) ).ReportTo( builder.Diagnostics );
        }
    }
    
    class Aspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Oops");
            return meta.Proceed();
        }
    }
    
    class MyAttribute : Attribute {}

    class TargetCode
    {
        [Aspect1]
        int Method(int a)
{
            Console.WriteLine("Oops");
            return a;
}
    }
}