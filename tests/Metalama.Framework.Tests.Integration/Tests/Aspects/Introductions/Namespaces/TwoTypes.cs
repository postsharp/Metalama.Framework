#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.TwoTypes;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var @namespace = builder.Advice.IntroduceNamespace(builder.Target.Compilation.GlobalNamespace, "Implementation");
        var @class1 = @namespace.IntroduceClass("TestClass1");
        var @class2 = @namespace.IntroduceClass("TestClass2");

        builder.IntroduceField("Field1", @class1.Declaration);
        builder.IntroduceField("Field2", @class2.Declaration);
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }