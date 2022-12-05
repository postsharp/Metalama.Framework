[Introduction]
internal class TargetClass
{
  public static EventHandler Foo = new EventHandler(Bar);
  public static void Bar(object? sender, EventArgs eventArgs)
  {
  }
  [method: global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
  public event global::System.EventHandler? IntroducedEventField = global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.InitializerTemplate_CrossAssembly.TargetClass.Foo;
  [method: global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
  public static event global::System.EventHandler? IntroducedEventField_Static = global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.InitializerTemplate_CrossAssembly.TargetClass.Foo;
}