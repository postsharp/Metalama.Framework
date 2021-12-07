// Warning MY001 on `Method1`: `Add some attribute`
//    CodeFix: Remove [My] from 'TargetCode'`
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
        static DiagnosticDefinition<None> _diag = new("MY001", Severity.Warning, "Add some attribute");

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);

            builder.Diagnostics.Report(builder.Target, _diag, default, CodeFix.RemoveAttributes(builder.Target.DeclaringType, typeof(MyAttribute)));
        }
    }

    class MyAttribute : Attribute { }
    class YourAttribute : Attribute { }

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