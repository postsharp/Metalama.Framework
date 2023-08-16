// Warning CS0162 on `return`: `Unreachable code detected`
internal class TargetCode
{
    [Aspect]
    private void Method()
    {
        global::System.Console.WriteLine("Shold return? False");
        global::System.Console.WriteLine("Shold return? True");
        return;
        return;
    }
}
