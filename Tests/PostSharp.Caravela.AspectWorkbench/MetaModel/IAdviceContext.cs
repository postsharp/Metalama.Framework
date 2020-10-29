namespace Caravela.AspectWorkbench
{
    internal interface IAdviceContext
    {
        IMethodAdviceContext MethodAdviceContext { get; }
        IProceedImpl ProceedImpl { get; }
    }
}