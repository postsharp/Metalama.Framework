internal class Target
{
  [return: MyAspect]
  string M1()
  {
    global::System.String returnValue;
    returnValue = "foo";
    foreach (var c in returnValue)
    {
      global::System.Console.WriteLine(c);
    }
    return returnValue;
  }
  [return: MyAspect]
  bool M2()
  {
    global::System.Boolean returnValue;
    returnValue = false;
    if (returnValue)
    {
      global::System.Console.WriteLine("T");
    }
    return returnValue;
  }
}