using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Bugs.TemplateParameterDiscard;

internal class Aspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Override( nameof(Template) );
    }

    [Template]
    private void Template( int arg )
    {
        _ = arg;
    }
}

// <target>
internal class Program
{
    [Aspect]
    private void M( int arg ) { }
}