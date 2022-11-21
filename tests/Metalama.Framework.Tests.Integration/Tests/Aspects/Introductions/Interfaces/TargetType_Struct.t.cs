// Warning CS0414 on `EventField`: `The field 'TargetStruct.EventField' is assigned but its value is never used`
[IntroduceAspect]
public struct TargetStruct : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TargetType_Struct.IInterface
{
  public TargetStruct()
  {
  }
  public int ExistingField = default;
  public int ExistingProperty { get; set; } = default;
  public void ExistingMethod()
  {
    Console.WriteLine("Original struct member");
  }
  public global::System.Int32 AutoProperty { get; set; } = default;
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
  public event global::System.EventHandler? EventField = default;
}