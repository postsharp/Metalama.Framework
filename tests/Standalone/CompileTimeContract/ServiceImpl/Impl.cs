using Contract;
using Metalama.Compiler;

namespace ServiceImpl
{
    [MetalamaPlugIn]
    public class Impl : IContract
    {
        public int Foo()
        {
            return 42;
        }
    }
}