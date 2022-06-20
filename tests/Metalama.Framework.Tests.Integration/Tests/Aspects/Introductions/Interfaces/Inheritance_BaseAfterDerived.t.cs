[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Inheritance_BaseAfterDerived.IDerivedInterface
{

    public global::System.Int32 Bar()
    {
        global::System.Console.WriteLine($"This is introduced interface member by Derived (should be Derived).");
        return default(global::System.Int32);
    }

    public global::System.Int32 Foo()
    {
        global::System.Console.WriteLine($"This is introduced interface member by Derived (should be Derived).");
        return default(global::System.Int32);
    }
}