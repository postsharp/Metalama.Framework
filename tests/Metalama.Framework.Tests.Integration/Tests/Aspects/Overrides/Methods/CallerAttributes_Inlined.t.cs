internal class TargetClass
{
  [Override]
  public void OverriddenMethod()
  {
    global::System.Console.WriteLine("This is the overridden method.");
    this.MethodWithCallerMemberName(42);
    this.MethodWithCallerMemberName(42, y: 27);
    this.MethodWithCallerMemberName(42, name1: "foo", y: 27);
    this.MethodWithCallerMemberName(42, "foo", 27);
    this.MethodWithCallerMemberName(42, "foo", 27, "bar");
    return;
  }
  public void MethodWithCallerMemberName(int x, [CallerMemberName] string name1 = "", int y = 0, [CallerMemberName] string name2 = "")
  {
  }
}