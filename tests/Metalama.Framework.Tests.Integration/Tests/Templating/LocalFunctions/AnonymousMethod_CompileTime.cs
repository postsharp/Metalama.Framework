using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.CompileTimeAnonymousMethod;

[CompileTime]
internal class Aspect
{
    [TestTemplate]
    private dynamic? Template()
    {
        var func = meta.CompileTime( new Func<string>( () => { return "Hello, world."; } ) );

        Console.WriteLine(func());
        return null;
    }
}

internal class TargetCode
{
    private int Method( int a )
    {
        return a;
    }
}