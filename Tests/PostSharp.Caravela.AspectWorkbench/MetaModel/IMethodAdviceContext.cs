using Caravela.Framework.Project;
using System.Collections.Generic;

namespace Caravela.AspectWorkbench
{
    [CompileTimeAttribute]
    public interface IMethodAdviceContext
    {
        string Name { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        dynamic Invoke { get; }

    }
}