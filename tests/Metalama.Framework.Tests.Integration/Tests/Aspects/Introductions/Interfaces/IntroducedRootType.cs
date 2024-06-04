#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.IntroducedRootType;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var type = builder.Advice.IntroduceClass(builder.Target.ContainingNamespace, "TestType", TypeKind.Class);
        type.ImplementInterface(typeof(ITestInterface));
    }
}

public interface ITestInterface { }

// <target>
[IntroductionAttribute]
public class TargetType { }