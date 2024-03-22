using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using System.ComponentModel;

#if TEST_OPTIONS
// @DependencyDefinedConstant(DEPENDENCY)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.GetDependencyOptions_CrossProject;

class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        var options = builder.Target.Enhancements().GetOptions<Options>();

        builder.Advice.IntroduceAttribute(builder.Target, AttributeConstruction.Create(typeof(DescriptionAttribute), new[] { options.ProjectPath }));
    }
}

// <target>
class Outer
{
    [Aspect]
    class Target : C
    {
    }
}