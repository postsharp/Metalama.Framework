using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.BugX;

// <target>
public class TargetType
{
    public int Foo(int x)
    {
        Console.WriteLine("Original");
        return x;
    }
    public void Foo_Void(int x)
    {
        Console.WriteLine("Original");
    }
}