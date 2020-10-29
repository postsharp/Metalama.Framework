using Caravela.Framework.Project;

namespace Caravela.AspectWorkbench
{
    [CompileTime]
    public interface IParameter
    {
        string Name { get; }
        dynamic Value { get; set; }
        
        bool IsOut { get; }
    }
}