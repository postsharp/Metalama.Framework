namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Iterator_CrossAssembly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

// <target>
[Test]
public class TestClass
{
    public IEnumerable<string> Enumerable( string text )
    {
        yield return "Hello";
        yield return text;
    }

    public IEnumerator<string> Enumerator(string text)
    {
        yield return "Hello";
        yield return text;
    }
}