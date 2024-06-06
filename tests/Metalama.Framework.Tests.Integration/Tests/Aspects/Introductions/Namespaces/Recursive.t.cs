// --- Recursive.cs ---
// Warning CS8618 on `Field`: `Non-nullable field 'Field' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
[IntroductionAttribute]
public class TargetType
{
  private global::Outer.Middle.Inner.Test Field;
}
// --- Outer.Middle.Inner.Test.cs ---
namespace Outer.Middle.Inner
{
  class Test : global::System.Object
  {
  }
}