internal class TargetCode
{
    private int Method1(int a) => a;
    private string Method2(string s)
    {
        global::System.Console.WriteLine("overridden");
        return s;
    }

    private string Property1
    {
        get
        {
            global::System.Console.WriteLine("overridden");
            return "";
        }
    }
}

namespace Sub
{
    internal class AnotherClass
    {
        private int Method1(int a) => a;
        private string Method2(string s)
        {
            global::System.Console.WriteLine("overridden");
            return s;
        }

        private string Property1
        {
            get
            {
                global::System.Console.WriteLine("overridden");
                return "";
            }
        }
    }
}
