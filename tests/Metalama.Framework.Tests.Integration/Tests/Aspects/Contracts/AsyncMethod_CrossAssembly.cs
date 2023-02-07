namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.AsyncMethod_CrossAssembly;

using System;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

// <target>
[Test]
public class TestClass
{
    public string DoSomething(string text)
    {
        Console.WriteLine("Hello");

        return null!;
    }

    public async Task DoSomethingAsync(string text)
    {
        await Task.Yield();

        Console.WriteLine("Hello");
    }

    public async Task<string> DoSomethingAsyncT(string text)
    {
        await Task.Yield();

        Console.WriteLine("Hello");

        return null!;
    }

    public async void DoSomethingAsyncVoid(string text)
    {
        await Task.Yield();

        Console.WriteLine("Hello");
    }
}