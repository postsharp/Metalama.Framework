// Warning CS8618 on `EventField`: `Non-nullable event 'EventField' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the event as nullable.`
namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTimeImplicitMembers
{
  partial class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTimeImplicitMembers.IInterface
  {
    public global::System.Int32 InterfaceMethod()
    {
      return default(global::System.Int32);
    }
    public global::System.Int32 AutoProperty { get; set; }
    public global::System.Int32 Property
    {
      get
      {
        return default(global::System.Int32);
      }
      set
      {
      }
    }
    public event global::System.EventHandler EventField;
    public event global::System.EventHandler Event
    {
      add
      {
      }
      remove
      {
      }
    }
  }
}