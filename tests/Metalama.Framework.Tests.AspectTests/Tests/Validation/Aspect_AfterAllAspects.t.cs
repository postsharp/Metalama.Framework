// Warning MY001 on `ValidatedClass`: `On 'ValidatedClass.IntroducedMethod()'.`
// Warning MY001 on `SourceMethod`: `On 'ValidatedClass.SourceMethod(object)'.`
[ValidateAspect]
[IntroduceAspect]
internal class ValidatedClass
{
  public static void SourceMethod(object o)
  {
  }
  public void IntroducedMethod()
  {
  }
}