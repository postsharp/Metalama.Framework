internal class Target
{
  private void Override([Test1][Test2][Test3] int p1, [Test1][Test2][Test3] int p2)
  {
    global::System.Console.WriteLine("Contract by aspect 1 on Target.Override(int, int)/p2, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 1 on Target.Override(int, int)/p2, ordinal 2");
    global::System.Console.WriteLine("Contract by aspect 1 on Target.Override(int, int)/p1, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 1 on Target.Override(int, int)/p1, ordinal 2");
    global::System.Console.WriteLine("Override by aspect 2 on Target.Override(int, int)/p2");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p2, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p2, ordinal 2");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p2, ordinal 3");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p2, ordinal 4");
    global::System.Console.WriteLine("Override by aspect 2 on Target.Override(int, int)/p1");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p1, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p1, ordinal 2");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p1, ordinal 3");
    global::System.Console.WriteLine("Contract by aspect 2 on Target.Override(int, int)/p1, ordinal 4");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.Override(int, int)/p2, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.Override(int, int)/p2, ordinal 2");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.Override(int, int)/p1, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.Override(int, int)/p1, ordinal 2");
    return;
  }
  private void NoOverride([Test1][Test3] int p1, [Test1][Test3] int p2)
  {
    global::System.Console.WriteLine("Contract by aspect 1 on Target.NoOverride(int, int)/p2, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 1 on Target.NoOverride(int, int)/p2, ordinal 2");
    global::System.Console.WriteLine("Contract by aspect 1 on Target.NoOverride(int, int)/p1, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 1 on Target.NoOverride(int, int)/p1, ordinal 2");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.NoOverride(int, int)/p2, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.NoOverride(int, int)/p2, ordinal 2");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.NoOverride(int, int)/p1, ordinal 1");
    global::System.Console.WriteLine("Contract by aspect 3 on Target.NoOverride(int, int)/p1, ordinal 2");
  }
}