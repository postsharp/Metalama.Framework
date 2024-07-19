// Warning CS8618 on `FieldWithExisting`: `Non-nullable field 'FieldWithExisting' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
// Warning CS8618 on `FieldWithIntroduced`: `Non-nullable field 'FieldWithIntroduced' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
[IntroductionAttribute]
public class TargetType
{
  public class ExistingNestedType
  {
  }
  public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsField.TargetType.ExistingNestedType FieldWithExisting;
  public global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsField.TargetType.IntroducedNestedType FieldWithIntroduced;
  public class IntroducedNestedType : global::System.Object
  {
  }
}