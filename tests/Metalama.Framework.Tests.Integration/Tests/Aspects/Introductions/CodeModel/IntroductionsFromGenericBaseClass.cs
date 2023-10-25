using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.CodeModel.IntroductionsFromGenericBaseClass;

[Inheritable]
public class MyInheritableAspectWhichIntroducesAMethod : TypeAspect
{
    [Introduce(WhenExists = OverrideStrategy.Ignore)]
    public void M() { }

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect(builder);

        var m = builder.Target.AllMethods.OfName("M").SingleOrDefault();

        if (m != null)
        {
            builder.Advice.IntroduceMethod(builder.Target, nameof(CallM), args: new { m });
        }
    }
    
    [Template]
    private void CallM(IMethod m)
    {
        m.Invoke();
    }
}

public class AspectWhichCallsTheMethod : TypeAspect
{
}

// <target>
[MyInheritableAspectWhichIntroducesAMethod]
class Foo<T> { }

// <target>
class Bar : Foo<int> { }