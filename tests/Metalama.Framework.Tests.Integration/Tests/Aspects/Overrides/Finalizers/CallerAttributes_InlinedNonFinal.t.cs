internal class TargetClass
{
  [Override]
  ~TargetClass()
  {
    this.Finalize_Override();
    global::System.Console.WriteLine("This is the overridden method.");
    this.Finalize_Override();
    return;
  }
  void Finalize_Override()
  {
    global::System.Console.WriteLine("This is the overridden method.");
    this.MethodWithCallerMemberName(42, name1: "Finalize", name2: "Finalize");
    this.MethodWithCallerMemberName(42, y: 27, name1: "Finalize", name2: "Finalize");
    this.MethodWithCallerMemberName(42, name1: "foo", y: 27, name2: "Finalize");
    this.MethodWithCallerMemberName(42, "foo", 27, name2: "Finalize");
    this.MethodWithCallerMemberName(42, "foo", 27, "bar");
    return;
  }
  public void MethodWithCallerMemberName(int x, [CallerMemberName] string name1 = "", int y = 0, [CallerMemberName] string name2 = "")
  {
  }
}