using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Tests.Integration.Tests.Options;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.BaseMethod;

public class C
{
    [MyOptions( "C.Method" )]
    [OptionsAspect]
    public virtual void Method() { }
}

public class D : C
{
    // <target>
    [OptionsAspect]
    public override void Method() { }
}