using Caravela.Framework.Impl.Templating.MetaModel;

namespace Caravela.TestFramework.MetaModel
{
    internal interface IAdviceContext
    {
        IMethodAdviceContext MethodAdviceContext { get; }
        IProceedImpl ProceedImpl { get; }
    }
}