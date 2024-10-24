using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.AnonymousMethod;

[CompileTime]
internal class Aspect
{
    [TestTemplate]
    private dynamic? Template()
    {
        object? result = null;
        RunTimeClass.Execute( delegate { result = meta.Proceed(); } );

        return result;
    }
}

internal class RunTimeClass
{
    public static void Execute( Action action ) => action();
}

internal class TargetCode
{
    private int Method( int a )
    {
        return a;
    }
}