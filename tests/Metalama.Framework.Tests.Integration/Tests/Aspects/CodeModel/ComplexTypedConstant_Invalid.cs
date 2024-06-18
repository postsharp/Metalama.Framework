#if TEST_OPTIONS
// @AcceptInvalidInput
#endif

using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.CodeModel.ComplexTypedConstant_Invalid;

public class Aspect : TypeAspect
{
    [Template]
    private int[] P { get; } = null!;

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var typedConstant = TypedConstant.Create( new[] { 42 } );
        builder.Advice.IntroduceParameter( builder.Target.Constructors.Single(), "p", typeof(int[]), typedConstant );
    }
}

// <target>
[Aspect]
internal class TargetCode { }