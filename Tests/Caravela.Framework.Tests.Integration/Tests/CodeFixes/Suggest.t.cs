using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.CodeFixes;
using System.ComponentModel;

namespace Caravela.Framework.Tests.Integration.CodeFixes.Suggest
{
    class Aspect : MethodAspect
    {
    
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            
            builder.Diagnostics.Suggest( CodeFix.AddAttribute(  builder.Target, typeof(MyAttribute) ) );
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
