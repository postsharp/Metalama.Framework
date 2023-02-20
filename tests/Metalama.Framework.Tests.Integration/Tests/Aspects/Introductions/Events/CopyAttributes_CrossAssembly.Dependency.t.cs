[Introduction]
internal class TargetClass
{
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
  public event global::System.EventHandler? Event
  {
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
    [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
    [param: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
    add
    {
      global::System.Console.WriteLine("Original add accessor.");
    }
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
    [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
    [param: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
    remove
    {
      global::System.Console.WriteLine("Original remove accessor.");
    }
  }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
  [method: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributes.FooAttribute]
  public event global::System.EventHandler? FieldLikeEvent;
}