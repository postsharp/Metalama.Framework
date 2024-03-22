#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.RefReadonlyParameter_Introduce;

class TheAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceMethod(builder.Target, nameof(Programmatic), buildMethod: method =>
        {
            method.AddParameter("i", typeof(int), RefKind.In);
            method.AddParameter("j", typeof(int), RefKind.RefReadOnly);
        });
    }

    [Introduce]
    void Declarative(in int i, ref readonly int j) { }

    [Template]
    void Programmatic() { }
}

[TheAspect]
class C
{
}

#endif