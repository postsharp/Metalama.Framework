using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.WithTemplateProvider;
#pragma warning disable CS0067

public class MyAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


}

#pragma warning restore CS0067
#pragma warning disable CS0067

    class TemplateProvider : ITemplateProvider
    {
        [Template]
        public int MyProperty { get; set; }
    }

#pragma warning restore CS0067


[MyAspect]
public class C { 

public global::System.Int32 MyProperty { get; set; }}
