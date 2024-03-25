#if TEST_OPTIONS
// @DesignTime
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DesignTime.NullableGenericSpecialType;

class MyAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        _ = builder.Target.ReturnType.SpecialType;
    }
}

// <target>
class C
{
    [MyAspect]
    T? MStruct<T>() where T : struct => null;

    [MyAspect]
    T? MClass<T>() where T : class => null;
}