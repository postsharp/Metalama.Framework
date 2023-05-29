namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Iterator_CrossAssembly;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

// <target>
[Test]
public class TestClass
{
    public IEnumerable? Enumerable(string text)
    {
        yield return "Hello";
        yield return text;
    }

    public IEnumerator Enumerator(string text)
    {
        yield return "Hello";
        yield return text;
    }

    public IEnumerable<string> EnumerableT( string text )
    {
        yield return "Hello";
        yield return text;
    }

    public IEnumerator<string> EnumeratorT(string text)
    {
        yield return "Hello";
        yield return text;
    }
}