#if TEST_OPTIONS
// @DesignTime
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DesignTime.NullableGenericSpecialType;

internal class MyAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        _ = builder.Target.ReturnType.SpecialType;
    }
}

// <target>
internal class C
{
    [MyAspect]
    private T? MStruct<T>() where T : struct => null;

    [MyAspect]
    private T? MClass<T>() where T : class => null;
}