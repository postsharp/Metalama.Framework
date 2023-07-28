using Metalama.Framework.Aspects;
using Metalama.Framework.Services;

[assembly: CompileTime]

namespace Contract
{
    public interface IContract : IProjectService
    {
        public int Foo();
    }
}