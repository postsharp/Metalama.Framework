[IntroduceAndInvoke]
internal class Target : global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.InvokingIntroduced.IFoo
{
  public void Bar()
  {
  }
  public void Introduce()
  {
  }
  public void Invoke()
  {
    this.Bar();
  }
  public void Invoke0()
  {
    this.Introduce();
  }
}