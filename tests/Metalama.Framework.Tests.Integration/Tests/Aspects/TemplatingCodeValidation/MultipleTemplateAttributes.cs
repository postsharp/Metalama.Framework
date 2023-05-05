using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.MultipleTemplateAttributes;

public class Aspect : TypeAspect
{
    [Introduce, Template]
    void M() { }
}