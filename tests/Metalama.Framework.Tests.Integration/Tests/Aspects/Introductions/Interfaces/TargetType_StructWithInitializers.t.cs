[IntroduceAspect]
public struct TargetStruct : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TargetType_StructWithInitializers.IInterface
{
  public TargetStruct()
  {
  }
  public int ExistingField = 42;
  public int ExistingProperty { get; set; } = 42;
  public void ExistingMethod()
  {
    Console.WriteLine("Original struct member");
  }
  public global::System.Int32 AutoProperty { get; set; }
  public global::System.Int32 Property
  {
    get
    {
      global::System.Console.WriteLine("Introduced interface member");
      return (global::System.Int32)42;
    }
    set
    {
      global::System.Console.WriteLine("Introduced interface member");
    }
  }
  public void IntroducedMethod()
  {
    global::System.Console.WriteLine("Introduced interface member");
  }
  public event global::System.EventHandler? Event
  {
    add
    {
      global::System.Console.WriteLine("Introduced interface member");
    }
    remove
    {
      global::System.Console.WriteLine("Introduced interface member");
    }
  }
  public event global::System.EventHandler? EventField;
}