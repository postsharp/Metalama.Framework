// @ApplyCodeFix

using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.CodeFixes;
using System.ComponentModel;

namespace Caravela.Framework.Tests.Integration.CodeFixes.RemoveAttribute
{
    class Aspect : MethodAspect
    {
        static DiagnosticDefinition<None> _diag = new ( "MY001", Severity.Warning, "Add some attribute" );
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            builder.Diagnostics.Report( builder.Target, _diag, default, CodeFix.RemoveAttribute(  builder.Target.DeclaringType, typeof(MyAttribute) ) );
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