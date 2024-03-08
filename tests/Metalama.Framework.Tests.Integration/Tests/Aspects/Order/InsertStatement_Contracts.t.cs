internal class Target
{
  [return: Test1, Test2]
  private int NoOverride([Test1, Test2] ref int p1, [Test1, Test2] ref int p2)
  {
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 2");
    global::System.Int32 returnValue;
    returnValue = 42;
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 2");
    global::System.Console.WriteLine($"Contract on {returnValue} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {returnValue} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {returnValue} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {returnValue} by aspect 2, ordinal 2");
    return returnValue;
  }
  [Override]
  [return: Test1, Test2]
  private int Override([Test1, Test2] ref int p1, [Test1, Test2] ref int p2)
  {
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 2");
    global::System.Int32 returnValue_1;
    global::System.Console.WriteLine($"Override.");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 2");
    global::System.Int32 returnValue;
    returnValue = 42;
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 2, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 2, ordinal 2");
    global::System.Console.WriteLine($"Contract on {returnValue} by aspect 2, ordinal 1");
    global::System.Console.WriteLine($"Contract on {returnValue} by aspect 2, ordinal 2");
    returnValue_1 = returnValue;
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p1} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {p2} by aspect 1, ordinal 2");
    global::System.Console.WriteLine($"Contract on {returnValue_1} by aspect 1, ordinal 1");
    global::System.Console.WriteLine($"Contract on {returnValue_1} by aspect 1, ordinal 2");
    return returnValue_1;
  }
}