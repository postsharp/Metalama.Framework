// Warning CS0162 on `return`: `Unreachable code detected`
internal class TargetCode
{
    [Aspect]
    private void Method()
    {
        return;
        return;
    }
}
