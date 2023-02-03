internal class TargetClass
{
  [InnerOverride]
  [OuterOverride]
  public int TargetMethod_ConflictBetweenOverrides()
  {
    int i_1 = 27;
    int i = 42;
    int j = 42;
    return 42;
  }
  [InnerOverride]
  [OuterOverride]
  public int TargetMethod_ConflictWithParameter(int i)
  {
    int i_2 = 27;
    int i_1 = 42;
    int j = 42;
    return 42;
  }
  [InnerOverride]
  [OuterOverride]
  public int TargetMethod_ConflictWithTarget()
  {
    int i_2 = 27;
    int i_1 = 42;
    int j = 42;
    int i = 0;
    return 42;
  }
  [InnerOverride]
  [OuterOverride]
  public int TargetMethod_MultipleConflicts()
  {
    int i_2 = 27;
    int i_1 = 42;
    int j_1 = 42;
    int i = 0;
    int j = 0;
    return 42;
  }
}