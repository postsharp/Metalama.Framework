using System;
using System.Threading.Tasks;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Tests.Integration.Aspects.Sdk.WeaverAndRegularAspects_AddAspect3;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[assembly: AspectOrder(typeof(RegularAspect1), typeof(WeaverAspect), typeof(CombinedAspect), typeof(RegularAspect2))]

namespace Metalama.Framework.Tests.Integration.Aspects.Sdk.WeaverAndRegularAspects_AddAspect3
{
    [RequireAspectWeaver("Metalama.Framework.Tests.Integration.Aspects.Sdk.WeaverAndRegularAspects_AddAspect3.AspectWeaver")]
    internal class WeaverAspect : MethodAspect { }

    // Weaver aspect is not actually used, so weaver does not have to exist.

    internal class RegularAspect1 : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine("Added by regular aspect #1.");

            return meta.Proceed();
        }
    }

    internal class RegularAspect2 : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine("Added by regular aspect #2.");

            return meta.Proceed();
        }
    }

    internal class CombinedAspect : MethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Outbound.AddAspect<RegularAspect1>();
            builder.Outbound.AddAspect<RegularAspect2>();
        }
    }

    // <target>
    internal class TargetCode
    {
        [CombinedAspect]
        private void M()
        {
        }
    }
}