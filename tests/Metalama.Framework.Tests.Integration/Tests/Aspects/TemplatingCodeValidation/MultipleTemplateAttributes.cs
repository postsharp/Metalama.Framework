using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.MultipleTemplateAttributes;

public class Aspect : TypeAspect
{
    [Introduce, Template]
    void M() { }
}