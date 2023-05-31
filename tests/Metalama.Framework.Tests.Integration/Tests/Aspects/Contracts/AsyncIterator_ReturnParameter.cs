#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.AsyncIterator_ReturnParameter;

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
            builder.Advice.AddContract(
                method.ReturnParameter,
                nameof(ValidateParameter));
        }
    }

    [Template]
    private async void ValidateParameter( dynamic? value )
    {
        Console.WriteLine($"Advice");

        if (meta.Target.Parameter.Type.Is(TypeFactory.GetType(SpecialType.IAsyncEnumerable_T).WithTypeArguments(TypeFactory.GetType(SpecialType.String))))
        {
            await foreach(var item in (IAsyncEnumerable<object?>)value!)
            {
                if (item is null)
                {
                    throw new ArgumentNullException("<return>");
                }
            }
        }
        else
        {
            while (await value!.MoveNextAsync())
            {
                if (value.Current is null)
                {
                    throw new ArgumentNullException("<return>");
                }
            }
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

        while (await enumerator.MoveNextAsync())
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