using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal interface IAdviceImplementation
    {
        AdviceResult ToResult( ICompilation compilation );
    }
}
