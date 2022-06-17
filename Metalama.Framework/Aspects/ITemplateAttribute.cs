using Metalama.Framework.Code;

namespace Metalama.Framework.Aspects;

[RunTimeOrCompileTime]
public interface ITemplateAttribute
{
    string? Name { get; }

    bool? IsVirtual { get; }

    bool? IsSealed { get; }

    Accessibility? Accessibility { get; }
}