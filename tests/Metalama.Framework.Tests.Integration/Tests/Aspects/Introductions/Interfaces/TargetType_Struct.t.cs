[IntroduceAspect]
public struct TargetStruct : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TargetType_Struct.IInterface
{
    public void ExistingMethod()
    {
        Console.WriteLine("Original struct member");
    }


    public void IntroducedMethod()
    {
        global::System.Console.WriteLine("Introduced interface member");
    }
}