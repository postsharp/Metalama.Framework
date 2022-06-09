public class Target
{
    [Test]
    public void VoidMethod()
    {
        global::System.Console.WriteLine("MyProperty");
        return;
    }

    [Test]
    public int Method()
    {
        global::System.Console.WriteLine("MyProperty");
        return default;
    }

    [Test]
    public T? Method<T>()
    {
        global::System.Console.WriteLine("MyProperty");
        return default;
    }
}