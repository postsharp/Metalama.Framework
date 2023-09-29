[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TemplateProvider.IInterface
{
  public void Bar()
  {
    global::System.Console.WriteLine("Introduced interface member");
  }
  public void Foo()
  {
    global::System.Console.WriteLine("Introduced interface member");
  }
}