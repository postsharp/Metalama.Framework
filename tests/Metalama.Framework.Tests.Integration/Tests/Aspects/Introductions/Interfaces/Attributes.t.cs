[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.IInterface
{
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute(default(global::System.String))]
  public global::System.Int32 AutoProperty {[global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute("Getter")]
    get; [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute("Setter")]
    set; }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute(default(global::System.String))]
  public global::System.Int32 Property
  {
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute("Getter")]
    get
    {
      return (global::System.Int32)42;
    }
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute("Setter")]
    set
    {
    }
  }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestInterfaceAttribute(default(global::System.String))]
  public void Method()
  {
    global::System.Console.WriteLine("Introduced interface member");
  }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute(default(global::System.String))]
  public event global::System.EventHandler? Event
  {
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute(default(global::System.String))]
    add
    {
    }
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute(default(global::System.String))]
    remove
    {
    }
  }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute(default(global::System.String))]
  [method: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Attributes.TestAspectAttribute(default(global::System.String))]
  public event global::System.EventHandler? EventField;
}