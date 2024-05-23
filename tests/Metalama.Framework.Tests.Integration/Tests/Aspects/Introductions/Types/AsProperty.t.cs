// Warning CS8618 on `PropertyWithExisting`: `Non-nullable property 'PropertyWithExisting' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`
// Warning CS8618 on `PropertyWithIntroduced`: `Non-nullable property 'PropertyWithIntroduced' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`
[IntroductionAttribute]
public class TargetType
{
  public class ExistingNestedType
  {
  }
  public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsProperty.TargetType.ExistingNestedType PropertyWithExisting { get; set; }
  public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsProperty.TargetType.IntroducedNestedType PropertyWithIntroduced { get; set; }
  public class IntroducedNestedType : global::System.Object
  {
  }
}