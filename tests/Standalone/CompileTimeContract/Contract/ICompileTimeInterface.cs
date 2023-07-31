using Metalama.Framework.Aspects;
using Metalama.Framework.Services;

// This is explicitly included because our tests disable generated assembly attributes. Otherwise, this would added by the package.
[assembly: CompileTime]

namespace Contract
{
    public interface IContract : IProjectService
    {
        public int Foo();
    }
}