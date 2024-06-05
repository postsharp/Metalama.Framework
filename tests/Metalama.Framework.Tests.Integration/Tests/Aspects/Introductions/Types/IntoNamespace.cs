﻿#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntoNamespace
{

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceClass(builder.Target.ContainingNamespace, "TestType");
        }
    }

    // <target>
    namespace TargetNamespace
    {
        [IntroductionAttribute]
        public class TargetType { }
    }
}