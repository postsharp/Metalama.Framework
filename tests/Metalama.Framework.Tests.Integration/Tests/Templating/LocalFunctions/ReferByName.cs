using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;


namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.ReferByName;

[CompileTime]
class Aspect
{
    [TestTemplate]
    void Template()
    {
        void TheLocalFunction(object? state)
        {
            meta.Proceed();
        }

        ThreadPool.QueueUserWorkItem( TheLocalFunction );
    }
}
    
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}