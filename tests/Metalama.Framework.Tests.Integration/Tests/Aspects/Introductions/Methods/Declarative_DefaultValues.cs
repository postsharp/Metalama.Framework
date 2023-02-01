using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.Declarative_DefaultValues;

public class IntroductionAttribute : TypeAspect
{
    [Introduce]
    public void IntroducedMethod(int id = 0, string s = "asdf", object? o = null, decimal d = 3.14m)
    {
        Console.WriteLine($"id = {id}, s = {s}, o = {o}, d = {d}");
    }
}

// <target>
[Introduction]
internal class TargetClass { }