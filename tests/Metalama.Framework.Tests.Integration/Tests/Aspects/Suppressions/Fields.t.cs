internal class TargetClass
{
  // CS0169 expected here.
  private int x;
#pragma warning disable CS0169, CS0649
  [SuppressWarning]
  private int y;
#pragma warning restore CS0169, CS0649
#pragma warning disable CS0169, CS0649
  [SuppressWarning]
  private int w, z;
#pragma warning restore CS0169, CS0649
}