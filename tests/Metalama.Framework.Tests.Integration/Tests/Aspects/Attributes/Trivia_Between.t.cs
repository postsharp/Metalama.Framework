[IntroduceAttributeAspect]
internal class IntroduceTarget
{
  // first
  [OldAttribute]
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia_Between.NewAttribute]
  // second
  private void M()
  {
  }
}