// @ApplyCodeFix

using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.CodeFixes;
using System.ComponentModel;
using Caravela.Framework.Tests.Integration.CodeFixes.ApplyAspect;

[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) )]

namespace Caravela.Framework.Tests.Integration.CodeFixes.ApplyAspect
{
    class Aspect1 : MethodAspect
    {
        static DiagnosticDefinition _diag = new DiagnosticDefinition( "MY001", Severity.Warning, "Apply Aspect2" );
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            builder.Diagnostics.Report( _diag, CodeFix.ApplyAspect( builder.Target, new Aspect2(), "Apply" ) );
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
            return a;
        }
    }
}