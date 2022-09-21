internal class TargetClass
    {
        [Log]
        public static int Add(int a, int b)
        {
            global::System.Console.WriteLine("TargetClass.Add(int, int) started.");
    try
    {
        global::System.Int32 result;
            if (a == 0)
                throw new ArgumentOutOfRangeException(nameof(a));
            result = a + b;
        global::System.Console.WriteLine("TargetClass.Add(int, int) succeeded.");
        return (global::System.Int32)result;
    }
    catch (global::System.Exception e)
    {
        global::System.Console.WriteLine("TargetClass.Add(int, int) failed: " + e.Message);
        throw;
    }
        }
    }