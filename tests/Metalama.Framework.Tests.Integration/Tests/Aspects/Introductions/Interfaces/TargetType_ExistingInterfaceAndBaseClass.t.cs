[IntroduceAspect]
public class TestClass : BaseClass, IExistingInterface
, global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.TargetType_ExistingInterfaceAndBaseClass.IIntroducedInterface
{
    public void ExistingMethod()
    {
        Console.WriteLine("Original interface member.");
    }

    public override void ExistingBaseMethod()
    {
        Console.WriteLine("Original base class member.");
    }


    public void IntroducedMethod()
    {
        global::System.Console.WriteLine("Introduced interface member.");
    }
}