// <target>
[Override]
internal class TargetClass
{
    public void TargetMethod()
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine("This is the original method.");
        return;
    }
}