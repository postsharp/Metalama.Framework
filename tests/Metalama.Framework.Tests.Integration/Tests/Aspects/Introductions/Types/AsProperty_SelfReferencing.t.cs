// Warning CS8618 on `Property`: `Non-nullable property 'Property' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`
[IntroductionAttribute]
public class TargetType
{
  public class ExistingNestedType
  {
  }
  public class IntroducedNestedType : global::System.Object
  {
    public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsProperty_SelfReferencing.TargetType.IntroducedNestedType Property { get; set; }
  }
}