using System;
using System.Threading.Tasks;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Tests.Integration.Aspects.Sdk.SkippedWeaverAndRegularAspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Tests weaver between two regular aspects that is never run (so it doesn't have to actually exist).

[assembly: AspectOrder(typeof(RegularAspect1), typeof(WeaverAspect), typeof(RegularAspect2))]

namespace Metalama.Framework.Tests.Integration.Aspects.Sdk.SkippedWeaverAndRegularAspects
{
    [RequireAspectWeaver("Metalama.Framework.Tests.Integration.Aspects.Sdk.SkippedWeaverAndRegularAspects.AspectWeaver")]
    internal class WeaverAspect : MethodAspect { }

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

    // <target>
    internal class TargetCode
    {
        [RegularAspect1]
        [RegularAspect2]
        private void M()
        {
        }
    }
}