// Warning MY001 on `Method1`: `Add some attribute`
//    CodeFix: Remove [My] from 'TargetCode'`
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
        static DiagnosticDefinition _diag = new DiagnosticDefinition( "MY001", Severity.Warning, "Add some attribute" );
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            builder.Diagnostics.Report( _diag, CodeFix.RemoveAttribute(  builder.Target.DeclaringType, typeof(MyAttribute) ) );
        }
    }
    
    class MyAttribute : Attribute {}
    class YourAttribute : Attribute {}

    partial class TargetCode
    {
        [Aspect]
        int Method1(int a)
        {
            return a;
        }
        int Method2(int a)
        {
            return a;
        }
        
     
    }
    
    partial class TargetCode
    {
       [Your]
        int Method3(int a)
        {
            return a;
        }
    }
}
