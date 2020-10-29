using System.Collections.Generic;

namespace Caravela.AspectWorkbench
{
    [BuildTimeOnly]
    public interface IMethodAdviceContext
    {
        string Name { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        dynamic Invoke { get; }

    }
}