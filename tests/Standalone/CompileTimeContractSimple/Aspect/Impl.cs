using Metalama.Compiler;
using Metalama.Framework.Services;

namespace Aspect
{
    [MetalamaPlugIn]
    public class Service : IProjectService
    {
        public int Foo()
        {
            return 42;
        }
    }
}