// @ApplyCodeFix

using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.CodeFixes;
using System.ComponentModel;

namespace Metalama.Framework.Tests.Integration.CodeFixes.RemoveAttribute
{
    class Aspect : MethodAspect
    {
        static DiagnosticDefinition _diag = new ( "MY001", Severity.Warning, "Add some attribute" );
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            _diag.WithCodeFixes( CodeFix.RemoveAttributes(  builder.Target.DeclaringType, typeof(MyAttribute) ) ).ReportTo( builder.Diagnostics );
        }
    }
    
    class MyAttribute : Attribute {}
    class YourAttribute : Attribute {}

    partial class TargetCode
    {
        [Aspect, My]
        int Method1(int a)
        {
            return a;
        }
        
        [My]
        int Method2(int a)
        {
            return a;
        }
        
     
    }
    
    partial class TargetCode
    {
       [My, Your]
        int Method3(int a)
        {
            return a;
        }
    }
}