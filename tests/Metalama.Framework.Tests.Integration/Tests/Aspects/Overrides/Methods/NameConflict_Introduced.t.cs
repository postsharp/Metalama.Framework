[InnerOverride]
[OuterOverride]
[Introduction]
internal class TargetClass
{
  public global::System.Object IntroducedMethod_ConflictBetweenOverrides()
  {
    int i_1 = 27;
    int i = 42;
    return default(global::System.Object);
  }
  public global::System.Object IntroducedMethod_ConflictWithParameter(global::System.Int32 i)
  {
    int i_2 = 27;
    int i_1 = 42;
    return default(global::System.Object);
  }
}