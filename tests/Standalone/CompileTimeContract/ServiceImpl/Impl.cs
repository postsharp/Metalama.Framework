using Contract;
using Metalama.Compiler;
using Metalama.Framework.Aspects;

namespace ServiceImpl
{
    [MetalamaPlugIn]
    [CompileTime]
    public class Impl : IContract
    {
        public int Foo()
        {
            return 42;
        }
    }
}