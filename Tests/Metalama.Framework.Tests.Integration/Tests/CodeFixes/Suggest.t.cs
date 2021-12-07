using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.CodeFixes;
using System.ComponentModel;

namespace Metalama.Framework.Tests.Integration.CodeFixes.Suggest
{
    class Aspect : MethodAspect
    {
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            builder.Diagnostics.Suggest( builder.Target, CodeFix.AddAttribute(  builder.Target, typeof(MyAttribute) ) );
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