internal class Program
    {
        private static void Main()
        {
            Add( 1, 1 );
        }

        [Log]
        private static int Add( int a, int b )
        {
    global::System.Console.WriteLine($"Program.Add(a = {{{a}}}, b = {{{b}}}) started.");
    try
    {
        global::System.Int32 result;
            result = a + b;
goto __aspect_return_1;
__aspect_return_1:        global::System.Console.WriteLine($"Program.Add(a = {{{a}}}, b = {{{b}}}) returned {result}.");
        return (global::System.Int32)result;
    }
    catch (global::System.Exception e)
    {
        global::System.Console.WriteLine($"Program.Add(a = {{{a}}}, b = {{{b}}}) failed: {e.Message}");
        throw;
    }
        }
    }