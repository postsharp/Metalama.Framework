namespace Caravela.AspectWorkbench
{
    [BuildTimeOnly]
    public interface IParameter
    {
        string Name { get; }
        dynamic Value { get; set; }
        
        bool IsOut { get; }
    }
}