// Warning CS8618 on `Field`: `Non-nullable field 'Field' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
[IntroductionAttribute]
public class TargetType
{
  public class ExistingNestedType
  {
  }
  public class IntroducedNestedType
  {
    public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsField_SelfReferencing.TargetType.IntroducedNestedType Field;
  }
}