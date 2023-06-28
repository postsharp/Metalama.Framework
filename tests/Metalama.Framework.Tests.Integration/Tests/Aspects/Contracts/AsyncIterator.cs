#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.AsyncIterator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

public sealed class TestAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        foreach (var method in builder.Target.Methods)
        {
            foreach (var parameter in method.Parameters)
            {
                builder.Advice.AddContract(
                    parameter,
                    nameof(ValidateParameter),
                    args: new { parameterName = parameter.Name } );
            }
        }
    }

    [Template]
    private void ValidateParameter( dynamic? value, [CompileTime] string parameterName )
    {
        Console.WriteLine($"Advice");

        if (value is null)
        {
            throw new ArgumentNullException( parameterName );
        }
    }
}

public class Program
{
    private static async Task TestMain()
    {
        const string text = "testText";
        var test = new TestClass();

        await foreach (var item in test.AsyncEnumerable(text))
        {
            Console.WriteLine($"{item};");
        }

        var enumerator = test.AsyncEnumerator(text);
    
        while(await enumerator.MoveNextAsync())
        {
            Console.WriteLine($"{enumerator.Current};");
        }
    }
}

// <target>
[Test]
public class TestClass
{
    public async IAsyncEnumerable<string> AsyncEnumerable( string text )
    {
        await Task.Yield();
        yield return "Hello";
        await Task.Yield();
        yield return text;
    }

    public async IAsyncEnumerator<string> AsyncEnumerator(string text)
    {
        await Task.Yield();
        yield return "Hello";
        await Task.Yield();
        yield return text;
    }
}