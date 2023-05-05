using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.MultipleTemplateAttributesInheritance;

public class Aspect : OverrideMethodAspect
{
    [Introduce]
    public override dynamic? OverrideMethod()
    {
        return null;
    }
}