using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Advising;

[CompileTime]
public interface IInterfaceImplementationAdviser
{
    INamedType Target { get; }
}