using Metalama.Framework.Aspects;
using Metalama.Framework.Services;

namespace Contract
{
    [CompileTime]
    public interface IContract : IProjectService
    {
        public int Foo();
    }
}