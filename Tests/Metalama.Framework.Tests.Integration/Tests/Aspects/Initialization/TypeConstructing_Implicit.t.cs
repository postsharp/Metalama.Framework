    [Aspect]
    public class TargetCode
    {
static TargetCode()
{
    TypeConstructing_Aspect();
}
        private int Method( int a )
        {
            return a;
        }


private static void TypeConstructing_Aspect()
{
    global::System.Console.WriteLine($"TargetCode: Aspect");
}    }