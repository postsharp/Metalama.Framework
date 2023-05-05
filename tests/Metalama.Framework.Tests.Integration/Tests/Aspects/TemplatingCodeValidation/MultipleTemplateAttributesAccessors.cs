using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.MultipleTemplateAttributesAccessors;

public class Aspect : TypeAspect
{
    [Template]
    int P
    {
        [Template]
        get => 42;
    }
}