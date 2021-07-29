using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Observers
{
    public interface ICompilationModelObserver : IService
    {
        void OnInitialCompilationModelCreated( ICompilation compilation );
    }
}