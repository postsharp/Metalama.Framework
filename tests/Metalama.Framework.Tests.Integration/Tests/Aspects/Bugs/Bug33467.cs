
using System;
using Castle.DynamicProxy.Generators;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33467;

public sealed class Test
{
    public void Foo(A a)
    {
        a.Bar(); // Analyzers crash on "Bar" or "a.Bar" when calling SemanticModel.GetSymbolInfo.
    }
} // Remove and add back this bracket to force Semantic classification.

file static class AExtensions
{
    public static A Bar(this A a) => a;
}

public class A { }