internal class TargetClass
{
  [OuterAspect]
  [InnerAspect]
  private int Method(int z)
  {
    int LocalFunction_1()
    {
      if (z == 27)
      {
        int LocalFunction()
        {
          if (z == 27)
          {
            if (z == 42)
            {
              _ = 27;
              goto __aspect_return_2;
            }
            Console.WriteLine("Original");
            _ = 42;
            goto __aspect_return_2;
            __aspect_return_2:
              return (global::System.Int32)42;
          }
          global::System.Console.WriteLine("Inner");
          return (global::System.Int32)27;
        }
        if (z == 27)
        {
          _ = (global::System.Int32)42;
          goto __aspect_return_1;
        }
        global::System.Console.WriteLine("Inner");
        _ = (global::System.Int32)LocalFunction();
        goto __aspect_return_1;
        __aspect_return_1:
          return (global::System.Int32)42;
      }
      global::System.Console.WriteLine("Outer");
      return (global::System.Int32)27;
    }
    if (z == 27)
    {
      return (global::System.Int32)42;
    }
    global::System.Console.WriteLine("Outer");
    return (global::System.Int32)LocalFunction_1();
  }
}