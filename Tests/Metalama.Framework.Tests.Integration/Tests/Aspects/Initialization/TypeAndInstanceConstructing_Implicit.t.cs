    [Aspect]
    public class TargetCode
    {
        public TargetCode()
        {
    Constructing_TypeConstructing_Aspect();
        }

        static TargetCode()
        {
    Constructing_TypeConstructing_Aspect();
        }

        private int Method(int a)
        {
            return a;
        }


private static void Constructing_TypeConstructing_Aspect()
{
    global::System.Console.WriteLine($"TargetCode: Aspect");
}    }