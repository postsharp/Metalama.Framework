// --- TwoTypes.cs ---
// Warning CS8618 on `Field1`: `Non-nullable field 'Field1' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
// Warning CS8618 on `Field2`: `Non-nullable field 'Field2' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
[IntroductionAttribute]
public class TargetType
{
  private global::Implementation.TestClass1 Field1;
  private global::Implementation.TestClass2 Field2;
}
// --- Implementation.TestClass1.cs ---
namespace Implementation
{
  class TestClass1 : global::System.Object
  {
  }
}
// --- Implementation.TestClass2.cs ---
namespace Implementation
{
  class TestClass2 : global::System.Object
  {
  }
}