[Aspect]
public class Target
{
  private void FromBaseCompilation()
  {
    global::System.Console.WriteLine(typeof(global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.IntroducedType.Target.IntroducedType).Name);
  }
  private void FromMutableCompilation()
  {
    global::System.Console.WriteLine(typeof(global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.IntroducedType.Target.IntroducedType).Name);
  }
  class IntroducedType : global::System.Object
  {
  }
}