using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33671;

[RunTimeOrCompileTime]
public interface IFace
{
    string? ProfileName { get; init; }
}