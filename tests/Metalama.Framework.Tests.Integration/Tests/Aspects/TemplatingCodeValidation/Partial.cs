using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.Partial;

public partial class Aspect : IAspect
{
    [Template]
    partial void Template(int i);

    partial void RunTimeMethod(int i);

    [CompileTime]
    partial void CompileTimeMethod(int i);
}