namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTimeImplicitMembers
{
  partial class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTimeImplicitMembers.IInterface
  {
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
    public event global::System.EventHandler? Event
    {
      add
      {
      }
      remove
      {
      }
    }
    public event global::System.EventHandler? EventField;
    public global::System.Int32 InterfaceMethod()
    {
      return default(global::System.Int32);
    }
  }
}