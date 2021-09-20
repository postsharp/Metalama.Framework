    [Introduction]
    public class TargetClass:global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DerivedAfterBase.IBaseInterface,global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DerivedAfterBase.IDerivedInterface    {


public global::System.Int32 Foo()
{
    global::System.Console.WriteLine("This is introduced interface member.");
    return default(global::System.Int32);
}

public global::System.Int32 Bar()
{
    global::System.Console.WriteLine("This is introduced interface member.");
    return default(global::System.Int32);
}    }