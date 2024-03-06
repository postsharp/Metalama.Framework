[Override]
internal class TargetClass
{
  int Z;
  [ConstructorOnly]
  public TargetClass([ParamOnly] int x)
  {
    this.Z = x;
    global::System.Console.WriteLine("This is the overridden constructor.");
  }
}